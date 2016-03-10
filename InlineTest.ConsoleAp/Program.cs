using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using InlineTest.Model.FileSystemInterop;

namespace InlineTest.ConsoleAp
{
    public static class Program
    {
        private const int TopCount = 5;
        private const string GeneratorName = "InlineTest.DataGenerator.exe";
        private const string DefaultFolderName = "Test";

        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Directory.CreateDirectory(DefaultFolderName); // Создаем папку если её не существует
            using (var watcher = new DirectoryWatcher(DefaultFolderName, "*.txt", TopCount))
            {
                watcher.Update += OnUpdate;
                watcher.Start();
                Process.Start(GeneratorName, DefaultFolderName);
                Console.ReadKey();
            }
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            var watcher = (DirectoryWatcher) sender;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Топ {0} самых популярных символов в папке '{1}'", TopCount, Path.Combine(Environment.CurrentDirectory, DefaultFolderName));
            Console.WriteLine();
            int i = 0;
            foreach (var pair in watcher.TopN)
            {
                Console.WriteLine("{0}: {1} (код {2,5}){3,10} шт", ++i, pair.Key, (int) pair.Key, pair.Value);
            }
            Console.WriteLine("Производится подсчет статистики в реальном времени. Для остановки нажмите любую клавишу . . .");
        }
    }
}
