using System.IO;
using System.IO.Compression;
using GZipArchiver.Tasks;

namespace GZipArchiver.Archiver
{
    public class DecompressTask : ITask
    {
        private byte[] _inputBytes;

        public DecompressTask(byte[] bytes)
        {
            _inputBytes = bytes;
        }

        public TaskResult Execute()
        {
            const int bufferSize = MemoryController.ArchiverBlockSize;
            byte[] buffer = new byte[bufferSize];
            using (var outputStream = new MemoryStream())
            {
                int readCount = 0;
                using (var decompressStream = new GZipStream(new MemoryStream(_inputBytes), CompressionMode.Decompress))
                {
                    do
                    {
                        readCount = decompressStream.Read(buffer, 0, bufferSize);
                        if (readCount > 0)
                            outputStream.Write(buffer, 0, readCount);
                    }
                    while (readCount > 0);
                }
                return new ArchiverTaskResult(outputStream.ToArray());
            }
        }
    }
}

