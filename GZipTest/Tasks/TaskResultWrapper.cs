namespace GZipArchiver.Tasks
{
    public class TaskResultWrapper
    {
        private long _taskId;
        private long _prevTaskId;
        private TaskResult _result;

        public TaskResultWrapper(long taskId, long prevTaskId, TaskResult result)
        {
            _taskId = taskId;
            _prevTaskId = prevTaskId;
            _result = result;
        }

        public long TaskId { get { return _taskId; } }

        public long PreviousTaskId { get { return _prevTaskId; } }

        public TaskResult Result { get { return _result; } }
    }
}
