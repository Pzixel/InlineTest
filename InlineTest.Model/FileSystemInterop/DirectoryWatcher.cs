using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InlineTest.Model.Domain;
using InlineTest.Model.Extensions;

namespace InlineTest.Model.FileSystemInterop
{
    public class DirectoryWatcher : IDisposable
    {
        public string Path { get; }
        public string Filter { get; }
        public ReadOnlyDictionary<char, int> Statistics => new ReadOnlyDictionary<char, int>(_globalStatistics);

        public event EventHandler Update = delegate { };

        private readonly FileSystemWatcher _watcher;
        private bool _disposed;
        private readonly Dictionary<FileDescriptor, FileData> _currentData = new Dictionary<FileDescriptor, FileData>();
        private readonly Dictionary<string, FileDescriptor> _pathToDescriptor = new Dictionary<string, FileDescriptor>(); // Т.к. в решении не используется БД, приходится делать индексы вручную.
        private readonly object _syncRoot = new object();
        private readonly Statistics<char> _globalStatistics = new Statistics<char>(); 
        private readonly ManualResetEventSlim _initialProcessGate = new ManualResetEventSlim();
        private bool _initialFolderProceeded;

        public DirectoryWatcher(string path, string filter)
        {
            Path = path;
            Filter = filter;

            _watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                NotifyFilter = NotifyFilters.LastWrite 
            };

            _watcher.Changed += (sender, args) => OnChanged(args.FullPath);
            _watcher.Deleted += (sender, args) => OnDeleted(args.FullPath);
        }

        public void Start()
        {
            if (!_initialFolderProceeded)
            {
                lock (_syncRoot)
                {
                    if (!_initialFolderProceeded) //double-check locking
                    {
                        var files = Directory.GetFiles(Path, Filter, SearchOption.TopDirectoryOnly);
                        foreach (string filePath in files)
                        {
                            Task.Run(() => OnNewFile(filePath));
                        }
                        _initialProcessGate.Set();
                        _initialFolderProceeded = true;
                    }
                }
            }

            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }

        private void OnNewFile(string path)
        {
            _initialProcessGate.Wait(); // Если мы еще не закончили обрабатывать файлы, которые были в папке, а в ней уже что-то меняют, сначала досчитываем, а потом смотрим, что произошло
            var file = XFile.ReadText(path).Select(char.ToUpper);
            var statistics = new Statistics<char>(file); // Если коллизий мало, то выгоднее сразу посчитать значение, потому что иначе это нужно делать под lock'ом и мы теряем всю многопоточность
            UpdateStatistics(path, statistics);
        }

        private void UpdateStatistics(string path, Statistics<char> statistics)
        {
            var descriptor = new FileDescriptor(path);
            lock (_syncRoot) // лочим всегда, т.к. статистику и хэш файла мы уже посчитали, поэтому осталось только обновить пару служебных таблиц, это можно сделать быстро, таким образом лок не должен сильно влиять на производительность
            {
                FileData existingFile;
                if (!_currentData.TryGetValue(descriptor, out existingFile))
                {
                    _currentData[descriptor] = new FileData(statistics, path);
                    _globalStatistics.Add(statistics);
                    OnUpdate();
                }
                else
                    existingFile.Paths.Add(path);
                _pathToDescriptor[path] = descriptor;
            }
        }

        private void OnChanged(string path)
        {
            FileDescriptor descriptor;
            if (!_pathToDescriptor.TryGetValue(path, out descriptor)) //Если нет дескриптора, значит новый файл и удалять еще нечего
            {
                OnNewFile(path);
                return;
            }
            if (File.GetLastWriteTime(path) <= descriptor.LastRead) // Если документ не изменялся, то это известный баг вотчера: http://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
                return;
            OnDeleted(path);
            OnNewFile(path);
        }

        private void OnDeleted(string path)
        {
            _initialProcessGate.Wait();
            lock (_syncRoot)
            {
                var descriptor = _pathToDescriptor[path];
                var fileData = _currentData[descriptor]; // Ищем связанные с текущим путем дубликаты. Приходится создавать дополнительные словари для быстрого поиска за О(1)
                if (fileData.Paths.Count > 1)
                {
                    fileData.Paths.Remove(path); //Если удалили один из дубликатов, то удаляем только это путь, а собранную статистику не трогаем
                }
                else
                {
                    var deletedStatistics = _currentData[descriptor];
                    _currentData.Remove(descriptor);
                    _globalStatistics.Remove(deletedStatistics.Statistics);
                    OnUpdate();
                }

                _pathToDescriptor.Remove(path);
            }
        }
        private void OnUpdate()
        {
            Update(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeInternal()
        {
            if (_disposed)
                return;
            _disposed = true;
            Stop();
            _watcher.Dispose();
        }

        ~DirectoryWatcher()
        {
            DisposeInternal();
        }
    }
}
