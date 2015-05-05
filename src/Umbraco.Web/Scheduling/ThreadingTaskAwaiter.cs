using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// This is used to return an awaitable instance from a Task without actually returning the 
    /// underlying Task instance since it shouldn't be mutable.
    /// </summary>
    internal class ThreadingTaskAwaiter
    {
        private readonly Task _task;

        public ThreadingTaskAwaiter(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            _task = task;
        }

        /// <summary>
        /// With a GetAwaiter declared it means that this instance can be awaited on with the await keyword
        /// </summary>
        /// <returns></returns>
        public TaskAwaiter GetAwaiter()
        {
            return _task.GetAwaiter();
        }

        /// <summary>
        /// Gets the status of the running task.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no running task.</exception>
        /// <remarks>Unless the AutoStart option is true, there will be no running task until
        /// a background task is added to the queue. Unless the KeepAlive option is true, there
        /// will be no running task when the queue is empty.</remarks>
        public TaskStatus Status
        {
            get { return _task.Status; }
        }
    }
}