using System;
using System.IO;
using System.Threading;

namespace GZipArchiver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isCompress = true;
            string inFile = "";
            string outFile = "";
            string parseError = ParseCommand(args, ref isCompress, ref inFile, ref outFile);
            if (parseError != null)
            {
                System.Console.WriteLine(parseError);
            }
            else
            {
                var archiver = new Archiver.Archiver();
                Thread consoleWaiterThread = new Thread(RunConsoleWaiter);
                consoleWaiterThread.IsBackground = true;
                if (isCompress)
                    archiver.PrepareToCompress(inFile, outFile);
                else
                    archiver.PrepareToDecompress(inFile, outFile);
                consoleWaiterThread.Start(archiver);
                archiver.Start();
                consoleWaiterThread.Join();
            }
        }
        /// <summary>
        /// Метод RunConsoleWaiter выводит в консоль сообщения об ожидании завершения работы программы, а
        /// также вызывает преждевремменное завершение программы по нажатию клавиши Escape
        /// </summary>
        /// <param name="archiverObject">GZip архиватор</param>
        private static void RunConsoleWaiter(object archiverObject)
        {
            Archiver.Archiver archiver = (Archiver.Archiver)archiverObject;
            System.Threading.Thread.Sleep(200);
            if (archiver.IsAlive)
            {
                System.Console.Write("ждите завершения операции ");
            }
            int i = 0;
            do
            {
                if (Console.KeyAvailable == true)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Escape)
                    {
                        // вызвать остановку приложения
                        archiver.Stop();
                        System.Console.WriteLine("Операция прервана!");
                        return;
                    }
                }
                if (i == 5)
                {
                    for (int j = i; j > 0; --j)
                    {
                        Console.Write("\b \b");
                    }
                    i = 0;
                }
                System.Console.Write(".");
                ++i;
                System.Threading.Thread.Sleep(200);
            } while (archiver.IsAlive);
            System.Console.WriteLine("\nОперация завершена!");
        }
        /// <summary>
        /// ParseCommand проверяет валидность входных аргументов программы
        /// </summary>
        /// <param name="args">аргументы, поданные на вход программе</param>
        /// <param name="isCompress">режим работы программы архиватор/разархиватор</param>
        /// <param name="inputFilePath">Путь для входного файла</param>
        /// <param name="outputFilePath">Путь для выходного файла</param>
        /// <returns>сообщение об ошибке</returns>
        private static string ParseCommand(string[] args, ref bool isCompress, ref string inputFilePath, ref string outputFilePath)
        {
            if (args.Length != 3)
            {
                return "Ошибка ввода данных! Команда должна содержать 3 параметра!";
            }
            switch (args[0])
            {
            case "compress":
                isCompress = true;
                break;
            case "decompress":
                isCompress = false;
                break;
            default:
                return "Ошибка ввода данных! Первый параметр должен быть compress/decompress!";
            }
            inputFilePath = args[1];
            outputFilePath = args[2];
            if (inputFilePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
            {
                return "Ошибка ввода данных! Указан неправильный путь для входного файла!";
            }
            else if (!File.Exists(inputFilePath))
            {
                return "Ошибка ввода данных! Указанный входной фыйл не существует!";
            }
            try
            {
                string fileName = System.IO.Path.GetFileName(outputFilePath);
                string fileDirectory = System.IO.Path.GetDirectoryName(outputFilePath);
                if (!Directory.Exists(fileDirectory))
                {
                    return "Ошибка ввода данных! Указан неправильный путь для выходного файла!";
                }
                if (File.Exists(outputFilePath))
                {
                    return "Ошибка ввода данных! Указанный выходной фыйл уже существует!";
                }
            }
            catch (ArgumentException)
            {
                return "Ошибка ввода данных! Указан неправильный путь для выходного файла!";
            }
            return null;
        }
    }
}
