using System;
using InlineTest.Model;

namespace InlineTest.ConsoleAp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var watcher = new DirectoryWatcher("Test", "*.txt"))
            {
                watcher.Update += OnUpdate;
                watcher.Start();
                Console.ReadKey();
            }
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            var watcher = (DirectoryWatcher) sender;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            foreach (var pair in watcher.Statistics)
            {
                Console.WriteLine("{0}\t{1}", pair.Key, pair.Value);
            }
            Console.WriteLine("Сбор статистики начат. Для остановки нажмите любую клавишу . . .");
        }
    }
}
