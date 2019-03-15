using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipArchiver.Tasks
{
    /// <summary>
    /// TaskResultQueue хранит очередь результатов выполнения "заданий" (ITask),
    /// </summary>
    public class TaskResultQueue : IDisposable
    {
        private List<TaskResultWrapper> _results;
        private long _expectedTaskId;
        private AutoResetEvent _whNewTaskResult;
        private AutoResetEvent _whDequeue;
        private readonly object _lockerQueue;
        private readonly object _lockerStopFlag;
        private readonly object _lockerWaitDequeue;
        private bool _stopFlag;
        private int _dequeueWaiterCount;

        public TaskResultQueue()
        {
            _results = new List<TaskResultWrapper>();
            _expectedTaskId = -1;
            _whNewTaskResult = new AutoResetEvent(false);
            _whDequeue = new AutoResetEvent(false);
            _lockerQueue = new object();
            _lockerStopFlag = new object();
            _lockerWaitDequeue = new object();
            _stopFlag = false;
            _dequeueWaiterCount = 0;
        }

        /// <summary>
        /// Добавляет элемент в очередь
        /// </summary>
        /// <param name="taskResult">Добавляемый элемент</param>
        public void Enqueue(TaskResultWrapper taskResult)
        {
            bool needWait = false;
            if (taskResult.Result != null)
                needWait = !MemoryController.CanTakeMemory();
            if (needWait)
            {
                lock (_lockerWaitDequeue)
                    _dequeueWaiterCount++;
                _whDequeue.WaitOne();
                lock (_lockerWaitDequeue)
                    _dequeueWaiterCount--;
            }
            lock (_lockerQueue)
            {
                InsertNewTaskResult(taskResult);
                _whNewTaskResult.Set();
            }
        }
        /// <summary>
        /// Проверяет, пуста ли очередь
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            lock (_lockerQueue)
                return _results.Count == 0;
        }
        /// <summary>
        /// Достает очередной элемент из очереди (согласно TaskId)
        /// </summary>
        /// <returns></returns>
        public TaskResultWrapper Dequeue()
        {
            TaskResultWrapper result = null;
            bool continueWait = false;
            if (IsEmpty())
            {
                do
                {
                    _whNewTaskResult.WaitOne();
                    if (StopFlag)
                        return new TaskResultWrapper(-1, -1, null);
                    lock (_lockerQueue)
                        continueWait = ((_results.Count == 0) || (_results[0].PreviousTaskId != _expectedTaskId));
                }
                while (continueWait);
            }
            else
            {
                lock (_lockerQueue)
                    continueWait = (_results[0].PreviousTaskId != _expectedTaskId);
                while (continueWait)
                {
                    _whNewTaskResult.WaitOne();
                    if (StopFlag)
                        return new TaskResultWrapper(-1, -1, null);
                    lock (_lockerQueue)
                        continueWait = (_results[0].PreviousTaskId != _expectedTaskId);
                }
            }
            lock (_lockerQueue)
            {
                result = _results[0];
                _results.RemoveAt(0);
                _expectedTaskId = result.TaskId;
            }
            lock (_lockerWaitDequeue)
            {
                if (_dequeueWaiterCount > 0)
                    _whDequeue.Set();
            }
            return result;
        }

        public void StopTryDequeue()
        {
            StopFlag = true;
            _whNewTaskResult.Set();
        }
        /// <summary>
        /// освобождает занятые ресурсы
        /// </summary>
        public void Dispose()
        {
            _whNewTaskResult.Close();
        }
        /// <summary>
        /// Сигнализирует, что необходимо прекратить добавлять элементы в очередь и пытаться извлечь из нее элементы
        /// </summary>
        private bool StopFlag
        {
            get
            {
                lock (_lockerStopFlag)
                    return _stopFlag;
            }
            set
            {
                lock (_lockerStopFlag)
                    _stopFlag = value;
            }
        }
        /// <summary>
        /// Добавляет элемент в очередь согласно его TaskId
        /// </summary>
        /// <param name="taskResult"></param>
        private void InsertNewTaskResult(TaskResultWrapper taskResult)
        {
            int i = 0;
            for (; i < _results.Count; ++i)
            {
                if (_results[i].TaskId > taskResult.TaskId)
                    break;
            }
            _results.Insert(i, taskResult);
        }
    }
}
