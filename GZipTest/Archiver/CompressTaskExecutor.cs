using System.IO;
using GZipArchiver.Tasks;

namespace GZipArchiver.Archiver
{
    /// <summary>
    /// DecompressTaskExecutor создает .gz архив для входного файла
    /// </summary>
    public class CompressTaskExecutor : TaskExecutor
    {
        private string _inputFilePath;
        private string _outputFilePath;

        public CompressTaskExecutor(string inputFilePath, string outputFilePath, int threadCount)
            : base(threadCount)
        {
            _inputFilePath = inputFilePath;
            _outputFilePath = outputFilePath;
        }
        /// <summary>
        /// Метод GenerateTasks разбивает входной поток данных по блокам (СompressTask) и отдает их на обработку рабочим потокам (Worker)
        /// </summary>
        protected override void GenerateTasks()
        {
            const int bufferSize = MemoryController.ArchiverBlockSize;
            byte[] buffer = new byte[bufferSize];
            byte[] readBytes;
            using (BinaryReader reader = new BinaryReader(File.Open(_inputFilePath, FileMode.Open)))
            {
                int readCount = 0;
                do
                {
                    if (StopFlag) break;
                    readCount = reader.Read(buffer, 0, bufferSize);
                    if (readCount > 0)
                    {
                        readBytes = new byte[readCount];
                        for (int i = 0; i < readCount; ++i)
                        {
                            readBytes[i] = buffer[i];
                        }
                        AddTask(new CompressTask(readBytes));
                        readBytes = null;
                    }
                }
                while (readCount > 0);
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
    }
}
