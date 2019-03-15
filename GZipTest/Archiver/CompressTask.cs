using System.IO;
using System.IO.Compression;
using GZipArchiver.Tasks;

namespace GZipArchiver.Archiver
{
    public class CompressTask : ITask
    {
        private byte[] _inputBytes;

        public CompressTask(byte[] bytes)
        {
            _inputBytes = bytes;
        }

        public TaskResult Execute()
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream compressStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    compressStream.Write(_inputBytes, 0, _inputBytes.Length);
                }
                return new ArchiverTaskResult(outputStream.ToArray());
            }
        }
    }
}
