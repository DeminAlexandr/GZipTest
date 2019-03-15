namespace GZipArchiver.Archiver
{
    public class Archiver
    {
        private Tasks.TaskExecutor _taskExecutor;
        private object _lockerIsAlive;
        private bool _isAlive;

        public Archiver()
        {
            _isAlive = false;
            _lockerIsAlive = new object();
        }

        public bool IsAlive
        {
            get
            {
                lock (_lockerIsAlive)
                    return _isAlive;
            }
            private set
            {
                lock (_lockerIsAlive)
                    _isAlive = value;
            }
        }

        public void PrepareToCompress(string inputFilePath, string outputFilePath)
        {
            IsAlive = true;
            _taskExecutor = new CompressTaskExecutor(inputFilePath, outputFilePath, MemoryController.GetWorkerThreadCount());
        }

        public void PrepareToDecompress(string inputFilePath, string outputFilePath)
        {
            IsAlive = true;
            _taskExecutor = new DecompressTaskExecutor(inputFilePath, outputFilePath, MemoryController.GetWorkerThreadCount());
        }

        public void Start()
        {
            _taskExecutor.Start();
            IsAlive = false;
        }

        public void Stop()
        {
            if ((_taskExecutor != null) && IsAlive)
                _taskExecutor.Stop();
        }
    }
}
