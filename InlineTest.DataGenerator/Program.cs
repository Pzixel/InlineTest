﻿using System;
using System.IO;
using System.Threading;

namespace InlineTest.DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 0)
            {
                Console.WriteLine("Не задан путь для генерации файлов! Программа прекращает работу.");
                Console.ReadKey();
                return;
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
                string fileName = $"InlineTest_{Guid.NewGuid()}.txt";
                Console.WriteLine("Генерирую файл из {0} символов '{1}', с именем {2}", count, toGenerate, fileName);
                string content = new string(toGenerate, count); // Генерируем специально в памяти для скорости, для больших строк лучше сразу писать в файл.
                File.WriteAllText(Path.Combine(unboxedPath, fileName), content);

                int toSleep = r.Next(500, 1000);
                Console.WriteLine("Сплю {0} миллисекунд", toSleep);
                Thread.Sleep(toSleep);
            }
        }
    }
}
