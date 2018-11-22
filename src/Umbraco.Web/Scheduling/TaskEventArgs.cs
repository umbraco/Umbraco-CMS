using System;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides arguments for task runner events.
    /// </summary>
    /// <typeparam name="T">The type of the task.</typeparam>
    public class TaskEventArgs<T> : EventArgs
        where T : IBackgroundTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskEventArgs{T}"/> class with a task.
        /// </summary>
        /// <param name="task">The task.</param>
        public TaskEventArgs(T task)
        {
            Task = task;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskEventArgs{T}"/> class with a task and an exception.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="exception">An exception.</param>
        public TaskEventArgs(T task, Exception exception)
        {
            Task = task;
            Exception = exception;
        }

        /// <summary>
        /// Gets or sets the task.
        /// </summary>
        public T Task { get; private set; }

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}