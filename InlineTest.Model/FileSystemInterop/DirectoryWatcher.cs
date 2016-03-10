using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InlineTest.Model.Collections;
using InlineTest.Model.Domain;
using InlineTest.Model.Extensions;

namespace InlineTest.Model.FileSystemInterop
{
    /// <summary>
    /// Класс мониторинга изменений файлов в папке
    /// </summary>
    public class DirectoryWatcher : IDisposable
    {
        public string Path { get; }
        public string Filter { get; }
        public ReadOnlyDictionary<char, int> Total
        {
            get
            {
                lock (_syncRoot) //делаем снапшот
                {
                    return new ReadOnlyDictionary<char, int>(_globalStatistics);
                }
            }
        }

        public IReadOnlyCollection<KeyValuePair<char, int>> TopN => _topN.ToList().AsReadOnly();

        /// <summary>
        /// Событие, вызываемое при изменении, добавлении или удалении файлов из папки
        /// </summary>
        public event EventHandler Update = delegate { };

        #region private fields
        private volatile object _syncRoot = new object();
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<FileDescriptor, FileData> _currentData = new Dictionary<FileDescriptor, FileData>();
        private readonly Dictionary<string, FileDescriptor> _pathToDescriptor = new Dictionary<string, FileDescriptor>(); // Т.к. в решении не используется БД, приходится делать индексы вручную.
        private readonly Statistics<char> _globalStatistics = new Statistics<char>();
        private readonly LimitedSizeSortedList<KeyValuePair<char, int>> _topN;
        private readonly ManualResetEventSlim _initialProcessGate = new ManualResetEventSlim();
        private bool _initialFolderProceeded;
        private bool _disposed;
        #endregion

        /// <summary>
        /// Создает объект, наблюдающий за папкой по пути {path}, используя фильтр {filter}, и сортирующий топ N значений.
        /// Для запуска необходимо вызвать метод Start
        /// </summary>
        /// <param name="path">Путь к папке с файлами</param>
        /// <param name="filter">Фильтр желаемых файлов</param>
        /// <param name="topCount">Количество позиций для сортировки</param>
        public DirectoryWatcher(string path, string filter, int topCount)
        {
            Path = path;
            Filter = filter;
            _topN = new LimitedSizeSortedList<KeyValuePair<char, int>>(FrequencyComparer.Instance, topCount);

            _watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName
            };

            _watcher.Changed += (sender, args) => OnChanged(args.FullPath);
            _watcher.Deleted += (sender, args) => OnDeleted(args.FullPath);
            _watcher.Renamed += (sender, args) => OnRenamed(args);
        }

        private void OnRenamed(RenamedEventArgs args)
        {
            var descriptor = _pathToDescriptor[args.OldFullPath];
            _pathToDescriptor.Remove(args.OldFullPath);
            _pathToDescriptor[args.FullPath] = descriptor;
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

        private void OnChanged(string path)
        {
            FileDescriptor descriptor;
            if (!_pathToDescriptor.TryGetValue(path, out descriptor)) //Если нет дескриптора, значит новый файл и удалять еще нечего
            {
                OnNewFile(path);
                return;
            }
            if (File.GetLastWriteTime(path) <= descriptor.LastWrite) // Если документ не изменялся, то это известный баг вотчера: http://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
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
        
        private void OnUpdate()
        {
            lock (_syncRoot)
            {
                _topN.Update(_globalStatistics);
                Update(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
                lock (_syncRoot)
                    if (!_disposed)
                    {
                        DisposeInternal();
                        GC.SuppressFinalize(this);
                    }
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
