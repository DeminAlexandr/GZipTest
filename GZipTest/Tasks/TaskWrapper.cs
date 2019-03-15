namespace GZipArchiver.Tasks
{
    public class TaskWrapper
    {
        private long _id;
        private long _prevTaskId;
        private ITask _task;

        public TaskWrapper(long id, long prevTaskId, ITask task)
        {
            _id = id;
            _prevTaskId = prevTaskId;
            _task = task;
        }

        public long Id { get { return _id; } }

        public long PreviousTaskId { get { return _prevTaskId; } }

        public ITask Task { get { return _task; } }

        public TaskResult Execute()
        {
            return _task.Execute();
        }
    }
}
