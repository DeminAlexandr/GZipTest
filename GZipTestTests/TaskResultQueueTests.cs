using Microsoft.VisualStudio.TestTools.UnitTesting;
using GZipArchiver.Archiver;
using GZipArchiver.Tasks;

namespace GZipTestTests
{
    [TestClass]
    public class TaskResultQueueTests
    {
        [TestMethod]
        public void TestIsEmpty()
        {
            var taskResultQueue = new TaskResultQueue();
            Assert.AreEqual(true, taskResultQueue.IsEmpty());
        }

        [TestMethod]
        public void TestEnqueueAndDeququeArchiverTaskResult()
        {
            var taskResultQueue = new TaskResultQueue();
            taskResultQueue.Enqueue(new TaskResultWrapper(0, -1, new ArchiverTaskResult(null)));
            Assert.AreEqual(false, taskResultQueue.IsEmpty());
            var taskResult = taskResultQueue.Dequeue();
            Assert.AreEqual(true, taskResultQueue.IsEmpty());
            Assert.IsNotNull(taskResult);
            Assert.AreEqual(0, taskResult.TaskId);
            Assert.AreEqual(-1, taskResult.PreviousTaskId);
        }

        [TestMethod]
        public void TestEnqueueAndDeququeFinalTaskResult()
        {
            var taskResultQueue = new TaskResultQueue();
            taskResultQueue.Enqueue(new TaskResultWrapper(0, -1, null));
            Assert.AreEqual(false, taskResultQueue.IsEmpty());
            var taskResult = taskResultQueue.Dequeue();
            Assert.AreEqual(true, taskResultQueue.IsEmpty());
            Assert.IsNotNull(taskResult);
            Assert.AreEqual(0, taskResult.TaskId);
            Assert.AreEqual(-1, taskResult.PreviousTaskId);
        }
    }
}
