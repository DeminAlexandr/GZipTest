using System.IO;
using System.Collections.Generic;
using GZipArchiver.Tasks;

namespace GZipArchiver.Archiver
{
    /// <summary>
    /// DecompressTaskExecutor осуществляет разархивирование .gz файла
    /// </summary>
    public class DecompressTaskExecutor : TaskExecutor
    {
        private string _inputFilePath;
        private string _outputFilePath;

        public DecompressTaskExecutor(string inputFilePath, string outputFilePath, int threadCount)
            : base(threadCount)
        {
            _inputFilePath = inputFilePath;
            _outputFilePath = outputFilePath;
        }
        /// <summary>
        /// Метод GenerateTasks разбивает входной поток данных по блокам (DecompressTask) и отдает их на обработку рабочим потокам (Worker)
        /// </summary>
        protected override void GenerateTasks()
        {
            var gzipBlock = new List<byte>(MemoryController.ArchiverBlockSize);
            var file = new FileInfo(_inputFilePath);
            long availableByteCount = file.Length;
            byte[] header = null;
            string invalidFileFormatMessage = "Ошибка чтения данных! Файл поврежден или имеет неправильный формат!";
            using (BinaryReader reader = new BinaryReader(File.Open(_inputFilePath, FileMode.Open)))
            {
                try
                {
                    ReadGZipHeader(reader, ref availableByteCount, gzipBlock, invalidFileFormatMessage);
                    header = gzipBlock.ToArray();
                    byte b;
                    while (availableByteCount > 0)
                    {
                        for (int i = 0; (i < header.Length) && (availableByteCount > 0); ++i)
                        {
                            b = reader.ReadByte();
                            gzipBlock.Add(b);
                            availableByteCount--;
                            if (header[i] != b)
                                i = -1;
                        }
                        if (availableByteCount > 0)
                        {
                            if (availableByteCount < 8)
                                throw new InvalidDataException(invalidFileFormatMessage);
                            gzipBlock.RemoveRange(gzipBlock.Count - header.Length, header.Length);
                            AddTask(new DecompressTask(gzipBlock.ToArray()));
                            gzipBlock.Clear();
                            gzipBlock.AddRange(header);
                        }
                        else
                        {
                            if (StopFlag) break;
                            AddTask(new DecompressTask(gzipBlock.ToArray()));
                        } 
                    }
                }
                catch (InvalidDataException ex)
                {
                    System.Console.WriteLine(ex.Message);
                    Stop();
                }
            }
        }
        /// <summary>
        /// Метод ConsumeTasks собирает итоговые данные со всех рабочих потоков и записывает в их в выходной файл
        /// </summary>
        protected override void ConsumeTasks()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(_outputFilePath, FileMode.CreateNew)))
            {
                TaskResult taskResult;
                do
                {
                    taskResult = GetNextTaskResult();
                    if (taskResult != null)
                        writer.Write(((ArchiverTaskResult)taskResult).OutputBytes);
                }
                while (taskResult != null);
            }
        }
        /// <summary>
        /// Метод ReadGZipHeader читает заголовок GZip-блока
        /// </summary>
        /// <param name="reader">Входной поток</param>
        /// <param name="availableByteCount">Доступное количество байт для считывания</param>
        /// <param name="byteList">список, считанных байтов</param>
        /// <param name="invalidFileFormatMessage">сообщение, показываемое в случае неправильного формата данных в файле</param>
        private void ReadGZipHeader(BinaryReader reader, ref long availableByteCount, List<byte> byteList, string invalidFileFormatMessage)
        {
            byte b = 0;
            if (availableByteCount < 18)
                throw new InvalidDataException(invalidFileFormatMessage);
            b = reader.ReadByte();
            if (b != 0x1f)
                throw new InvalidDataException(invalidFileFormatMessage);
            byteList.Add(b);
            b = reader.ReadByte();
            if (b != 0x8b)
                throw new InvalidDataException(invalidFileFormatMessage);
            byteList.Add(b);
            for (int i = 0; i < 2; ++i)
            {
                b = reader.ReadByte();
                byteList.Add(b);
            }
            bool flagText = GetBit(b, 0);
            bool flagHCRC = GetBit(b, 1);
            bool flagExtra = GetBit(b, 2);
            bool flagName = GetBit(b, 3);
            bool flagComment = GetBit(b, 4);
            for (int i = 0; i < 6; ++i)
            {
                b = reader.ReadByte();
                byteList.Add(b);
            }
            availableByteCount -= 10;
            if (flagExtra)
            {
                if (availableByteCount < 10)
                    throw new InvalidDataException(invalidFileFormatMessage);
                b = reader.ReadByte();
                byteList.Add(b);
                int xlen = b;
                b = reader.ReadByte();
                byteList.Add(b);
                xlen = xlen + b * 256;
                if (availableByteCount < 10 + xlen)
                    throw new InvalidDataException(invalidFileFormatMessage);
                for (int i = 0; i < xlen; ++i)
                {
                    b = reader.ReadByte();
                    byteList.Add(b);
                }
                availableByteCount = availableByteCount - xlen - 2; 
            }
            if (flagName)
            {
                if (availableByteCount < 9)
                    throw new InvalidDataException(invalidFileFormatMessage);
                do
                {
                    b = reader.ReadByte();
                    byteList.Add(b);
                    availableByteCount--;
                }
                while ((b != 0) && (availableByteCount > 0));
                if (availableByteCount < 8)
                    throw new InvalidDataException(invalidFileFormatMessage);
            }
            if (flagComment)
            {
                if (availableByteCount < 9)
                    throw new InvalidDataException(invalidFileFormatMessage);
                do
                {
                    b = reader.ReadByte();
                    byteList.Add(b);
                    availableByteCount--;
                }
                while ((b != 0) && (availableByteCount > 0));
                if (availableByteCount < 8)
                    throw new InvalidDataException(invalidFileFormatMessage);
            }
            if (flagHCRC)
            {
                if (availableByteCount < 10)
                    throw new InvalidDataException(invalidFileFormatMessage);
                for (int i = 0; i < 2; ++i)
                {
                    b = reader.ReadByte();
                    byteList.Add(b);
                }
                availableByteCount -= 2;
            }
        }

        /// <summary>
        /// Метод GetBit возвращает значение выбранного бита в байте
        /// </summary>
        /// <param name="b">байт</param>
        /// <param name="number">номер бита в байте от 0 до 7 (нумерация справа-налево)</param>
        /// <returns>возвращаемое значение бита</returns>
        private bool GetBit(byte b, int number)
        {
            return (b & (1 << number)) != 0;
        }
    }
}
