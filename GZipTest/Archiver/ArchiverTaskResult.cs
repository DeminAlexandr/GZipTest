using GZipArchiver.Tasks;

namespace GZipArchiver.Archiver
{
    public class ArchiverTaskResult : TaskResult
    {
        private byte[] _outputBytes;

        public ArchiverTaskResult(byte[] bytes)
        {
            _outputBytes = bytes;
        }

        public byte[] OutputBytes { get { return _outputBytes; } }
    }
}
