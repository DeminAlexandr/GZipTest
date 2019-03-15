namespace GZipArchiver.Tasks
{
    public abstract class TaskExecutor
    {
        private WorkerPool _workerPool;
        private readonly object _lockerStopFlag;
        private bool _stopFlag;

        public TaskExecutor(int threadCount)
        {
            _workerPool = new WorkerPool(threadCount, ConsumeTasks);
            _lockerStopFlag = new object();
            _stopFlag = false;
        }

        protected bool StopFlag
        {
            get
            {
                lock (_lockerStopFlag)
                    return _stopFlag;
            }
        }

        public void Start()
        {
            _workerPool.Start(GenerateTasks);
        }

        public void Stop()
        {
            lock (_lockerStopFlag)
                _stopFlag = true;
            _workerPool.Stop();
        }

        protected void AddTask(ITask task)
        {
            _workerPool.AddTask(task);
        }

        protected TaskResult GetNextTaskResult()
        {
            return _workerPool.GetNextTaskResult();
        }

        protected abstract void GenerateTasks();

        protected abstract void ConsumeTasks();
    }
}
