using System;
using System.Threading;

namespace GZipArchiver.Tasks
{
    /// <summary>
    /// Worker работает в отдельном потоке и обрабатывает задачи (ITask), пока не
    /// будет остановлен с помощью метода Stop
    /// </summary>
    public class Worker : IDisposable
    {
        public delegate void SendFreeWorkerSignalDelegate();
        public delegate void ExecuteTaskDelegate(TaskWrapper task);

        private Thread _thread;
        private TaskWrapper _currTask;
        private AutoResetEvent _whReady;
        private AutoResetEvent _whNewTask;
        private readonly object _lockerTaskExecuted;
        private readonly object _lockerIsStopped;
        private readonly object _lockerIsDisposed;
        private bool _isTaskExecuted;
        private bool _isStopped;
        private bool _whNewTaskDisposed;
        private SendFreeWorkerSignalDelegate _sendFreeWorkerSignal;
        private ExecuteTaskDelegate _executeTask;

        public Worker(SendFreeWorkerSignalDelegate sendFreeWorkerSignal, ExecuteTaskDelegate executeTask)
        {
            _thread = new Thread(DoWork);
            _whReady = new AutoResetEvent(false);
            _whNewTask = new AutoResetEvent(false);
            _lockerTaskExecuted = new object();
            _lockerIsStopped = new object();
            _lockerIsDisposed = new object();
            _isTaskExecuted = false;
            _isStopped = false;
            _whNewTaskDisposed = false;
            _sendFreeWorkerSignal = sendFreeWorkerSignal;
            _executeTask = executeTask;
        }
        /// <summary>
        /// IsTaskExecuted сигнализирует, занят ли Worker обработкой какой-либо задачи
        /// </summary>
        public bool IsTaskExecuted
        {
            get
            {
                lock (_lockerTaskExecuted)
                    return _isTaskExecuted;
            }
            private set
            {
                lock (_lockerTaskExecuted)
                    _isTaskExecuted = value;
            }
        }
        /// <summary>
        /// Сигнализирует, что необходимо завершить работу потока
        /// </summary>
        private bool IsStopped
        {
            get
            {
                lock (_lockerIsStopped)
                    return _isStopped;
            }
            set
            {
                lock (_lockerIsStopped)
                    _isStopped = value;
            }
        }
        /// <summary>
        /// Добавляет новую задачу, если Worker не занят обработкой другой задачи
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool AddTask(TaskWrapper task)
        {
            if (IsTaskExecuted) return false;
            IsTaskExecuted = true;
            _currTask = task;
            _whNewTask.Set();
            return true;
        }
        /// <summary>
        /// Запускает поток
        /// </summary>
        public void Start()
        {
            _thread.Start();
            _whReady.WaitOne();
        }
        /// <summary>
        /// Подает сигнал для остановки Worker'а
        /// </summary>
        public void Stop()
        {
            IsStopped = true;
            lock (_lockerIsDisposed)
            {
                if (!_whNewTaskDisposed)
                    _whNewTask.Set();
            }
        }
        /// <summary>
        /// Освобождает русурсы и ожидает завершения потока Worker'а
        /// </summary>
        public void Dispose()
        {
            _thread.Join();
            lock (_lockerIsDisposed)
            {
                _whNewTask.Close();
                _whNewTaskDisposed = true;
            }
            _whReady.Close();
        }
        /// <summary>
        /// DoWork ожидает задачи и выполняет их по мере поступления
        /// </summary>
        private void DoWork()
        {
            _whReady.Set();
            _sendFreeWorkerSignal();
            while (!IsStopped)
            {
                _whNewTask.WaitOne();
                if (IsStopped) break;
                _executeTask(_currTask);
                IsTaskExecuted = false;
                _sendFreeWorkerSignal();
            }
        }
    }
}
