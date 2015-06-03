using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Wraps a Task within an object that gives access to its GetAwaiter method and Status
    /// property while ensuring that it cannot be modified in any way.
    /// </summary>
    internal class ThreadingTaskImmutable
    {
        private readonly Task _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadingTaskImmutable"/> class with a Task.
        /// </summary>
        /// <param name="task">The task.</param>
        public ThreadingTaskImmutable(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            _task = task;
        }

        /// <summary>
        /// Gets an awaiter used to await the task.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        public TaskAwaiter GetAwaiter()
        {
            return _task.GetAwaiter();
        }

        /// <summary>
        /// Gets the TaskStatus of the task.
        /// </summary>
        /// <returns>The current TaskStatus of the task.</returns>
        public TaskStatus Status
        {
            get { return _task.Status; }
        }
    }
}