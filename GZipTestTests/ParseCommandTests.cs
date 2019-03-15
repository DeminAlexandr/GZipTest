using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTestTests
{
    [TestClass]
    public class ParseCommandTests
    {
        [TestMethod]
        public void TestMoreCommandArguments()
        {
            string[] args = new string[4];
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Команда должна содержать 3 параметра!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [TestMethod]
        public void TestLessCommandArguments()
        {
            string[] args = new string[2];
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Команда должна содержать 3 параметра!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [TestMethod]
        public void TestInvalidFirstArgument()
        {
            string[] args = new string[3];
            args[0] = "test";
            args[1] = "";
            args[2] = "";
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Первый параметр должен быть compress/decompress!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [TestMethod]
        public void TestInvalidInputFilePathChars()
        {
            string[] args = new string[3];
            args[0] = "compress";
            args[1] = "<test>.txt";
            args[2] = "";
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Указан неправильный путь для входного файла!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [TestMethod]
        public void TestNotExistInputFile()
        {
            string[] args = new string[3];
            args[0] = "compress";
            args[1] = "test.txt";
            args[2] = "";
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Указанный входной фыйл не существует!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [Ignore]
        [TestMethod]
        public void TestInvalidOutputFilePathChars()
        {
            string[] args = new string[3];
            args[0] = "compress";
            args[1] = "W:\\Test\\test.txt";
            args[2] = "<test>.txt";
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Указан неправильный путь для выходного файла!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [Ignore]
        [TestMethod]
        public void TestExistOutputFile()
        {
            string[] args = new string[3];
            args[0] = "compress";
            args[1] = "W:\\Test\\test.txt";
            args[2] = "W:\\Test\\test.gz";
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Указанный выходной фыйл уже существует!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        [Ignore]
        [TestMethod]
        public void TestInputAndOutputFilesAreTheSame()
        {
            string[] args = new string[3];
            args[0] = "compress";
            args[1] = "W:\\Test\\test.txt";
            args[2] = "W:\\Test\\test.txt";
            bool isCompress = true;
            string inputFilePath = "";
            string outputFilePath = "";
            Assert.AreEqual("Ошибка ввода данных! Указанный выходной фыйл уже существует!", ParseCommand(args, ref isCompress, ref inputFilePath, ref outputFilePath));
        }

        private string ParseCommand(string[] args, ref bool isCompress, ref string inputFilePath, ref string outputFilePath)
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
