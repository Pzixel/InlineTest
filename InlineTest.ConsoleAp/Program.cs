using System;
using System.Diagnostics;
using System.Linq;
using InlineTest.Model;
using InlineTest.Model.FileSystemInterop;

namespace InlineTest.ConsoleAp
{
    class Program
    {
        const string GeneratorName = "InlineTest.DataGenerator.exe";

        static void Main(string[] args)
        {
            const int max = 5;
            using (var watcher = new DirectoryWatcher("Test", "*.txt", max))
            {
                watcher.Update += OnUpdate;
                watcher.Start();
                Process.Start(GeneratorName, "Test");
                Console.ReadKey();
            }
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            var watcher = (DirectoryWatcher) sender;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            foreach (var pair in watcher.TopN)
            {
                Console.WriteLine("{0}\t{1}", pair.Key, pair.Value);
            }
            Console.WriteLine("Сбор статистики начат. Для остановки нажмите любую клавишу . . .");
            foreach (var pair in watcher.Total.OrderByDescending(x=>x.Value).Take(5))
            {
                Console.WriteLine("{0}\t{1}", pair.Key, pair.Value);
            }
        }
    }
}
