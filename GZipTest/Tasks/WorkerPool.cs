using System;
using System.Threading;

namespace GZipArchiver.Tasks
{
    /// <summary>
    /// WorkerPool хранит массив Worker'ов,
    /// отвечает за распределение задач между Worker'ами и сбор результатов от различных Worker'ов
    /// </summary>
    public class WorkerPool : IDisposable
    {
        private TaskResultQueue _tasksResults;
        private Thread _consumerThread;
        private Worker[] _workers;
        private int _workedWorkerCount;
        private AutoResetEvent _whFreeWorkers;
        private long lastGeneratedTaskId;
        private readonly object _lockerStopFlag;
        private bool _stopFlag;
        /// <summary>
        /// Подается в TaskResultQueue последней, сообщает о завершении обработки всех задач (ITask)
        /// </summary>
        private class FinalTask : ITask
        {
            public TaskResult Execute()
            {
                return null;
            }
        }

        public WorkerPool(int threadCount, ThreadStart consumeTasks)
        {
            _tasksResults = new TaskResultQueue();
            _consumerThread = new Thread(consumeTasks);
            _workers = new Worker[threadCount];
            _workedWorkerCount = 0;
            _whFreeWorkers = new AutoResetEvent(false);
            lastGeneratedTaskId = -1;
            _lockerStopFlag = new object();
            _stopFlag = false;
        }
        /// <summary>
        /// Запускает функцию генерации задач и поток по обработке выполнения задач 
        /// </summary>
        /// <param name="generateTasks">функция генерации задач</param>
        public void Start(System.Action generateTasks)
        {
            _consumerThread.Start();
            if (!StopFlag)
                generateTasks();
            if (!StopFlag)
                AddTask(new FinalTask());
            _consumerThread.Join();
            Dispose();
        }
        /// <summary>
        /// Останавливает работу всех Worker'ов
        /// </summary>
        public void Stop()
        {
            StopFlag = true;
            _tasksResults.StopTryDequeue();
        }
        /// <summary>
        /// Добавляет новую задачу
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(ITask task)
        {
            long prevTaskId = lastGeneratedTaskId;
            var taskWrapper = new TaskWrapper(GenerateTaskId(), prevTaskId, task);
            Worker worker = GetFreeWorker();
            worker.AddTask(taskWrapper);
        }
        /// <summary>
        /// Ожидает и выдает результаты очередной задачи
        /// </summary>
        /// <returns></returns>
        public TaskResult GetNextTaskResult()
        {
            var taskResultWrapper = _tasksResults.Dequeue();
            return taskResultWrapper.Result;
        }
        /// <summary>
        /// Сигнализирует о необходимости остановить генерацию задач
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
        /// Сигнализирует, что один из Worker'ов освободился
        /// </summary>
        private void SendFreeWorkerSignal()
        {
            _whFreeWorkers.Set();
        }
        /// <summary>
        /// Служит для генерации TaskId
        /// </summary>
        /// <returns></returns>
        private long GenerateTaskId()
        {
            ++lastGeneratedTaskId;
            return lastGeneratedTaskId;
        }
        /// <summary>
        /// Выполняет задачу и добавляет результат в очередь 
        /// </summary>
        /// <param name="task"></param>
        private void ExecuteTask(TaskWrapper task)
        {
            var taskResult = task.Execute();
            _tasksResults.Enqueue(new TaskResultWrapper(task.Id, task.PreviousTaskId, taskResult));
        }
        /// <summary>
        /// Освобождает ресурсы и ожидает завершения всех Worker'ов
        /// </summary>
        public void Dispose()
        {
            StopWorkers();
            _tasksResults.Dispose();
            _whFreeWorkers.Close();
        }
        /// <summary>
        /// Останавливает всех Worker'ов
        /// </summary>
        private void StopWorkers()
        {
            bool hasWorkedThreads;
            do
            {
                hasWorkedThreads = false;
                for (int i = 0; i < _workers.Length; ++i)
                {
                    if (_workers[i] != null)
                    {
                        if (_workers[i].IsTaskExecuted)
                            hasWorkedThreads = true;
                        else
                        {
                            _workers[i].Stop();
                            _workers[i].Dispose();
                        }
                    }
                }
            }
            while (hasWorkedThreads);
        }
        /// <summary>
        /// Ожидает пока какой-либо Worker освободится и возвращает ссылку на него
        /// </summary>
        private Worker GetFreeWorker()
        {
            if (_workedWorkerCount < _workers.Length)
            {
                for (int i = 0; i < _workers.Length; ++i)
                {
                    if (_workers[i] == null)
                    {
                        ++_workedWorkerCount;
                        _workers[i] = new Worker(SendFreeWorkerSignal, ExecuteTask);
                        _workers[i].Start();
                        _whFreeWorkers.WaitOne();
                        return _workers[i];
                    }
                }
            }
            else
            {
                _whFreeWorkers.WaitOne();
                for (int i = 0; i < _workers.Length; ++i)
                {
                    if (!_workers[i].IsTaskExecuted)
                        return _workers[i];
                }
            }
            return null;
        }
    }
}
