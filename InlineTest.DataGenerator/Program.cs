using System;
using System.IO;
using System.Threading;

namespace InlineTest.DataGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
#if DEBUG
                args = new[] {""};
#else
                Console.WriteLine("Не задан путь для генерации файлов! Программа прекращает работу.");
                Console.ReadKey();
                return;
#endif
            }
            string path = args[0];
            Console.WriteLine("Началась генерация файлов в папке '{0}'. Для выхода нажмите любую клавишу . . .", path);
            var thread = new Thread(GenerateFiles)
            {
                IsBackground = true
            };
            thread.Start(path);
            Console.ReadKey();
        }

        private static void GenerateFiles(object path)
        {
            string unboxedPath = (string) path;
            var r = new Random();
            while (true)
            {
                char toGenerate = (char)r.Next('A', 'z');
                int count = r.Next(10, 1024 * 1024);
                string fileName = GetFileName(toGenerate, count);
                Console.WriteLine("Генерирую файл из {0} символов '{1}', с именем {2}", count, toGenerate, fileName);
                string content = new string(toGenerate, count); // Генерируем специально в памяти для скорости, для больших строк лучше сразу писать в файл.
                File.WriteAllText(Path.Combine(unboxedPath, fileName), content);

                int toSleep = r.Next(50, 1000);
                Console.WriteLine("Сплю {0} миллисекунд", toSleep);
                Thread.Sleep(toSleep);
            }
        }

        private static string GetFileName(char toGenerate, int count)
        {
            var charRepresentation = char.IsLetterOrDigit(toGenerate) ? toGenerate.ToString() : ((int) toGenerate).ToString();
            string result = $"test_{charRepresentation}_{count}_{Guid.NewGuid()}.txt";
            return result;
        }
    }
}
