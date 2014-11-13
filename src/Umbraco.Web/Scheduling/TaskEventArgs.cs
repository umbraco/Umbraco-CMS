using System;

namespace Umbraco.Web.Scheduling
{
    internal class TaskEventArgs<T> : EventArgs
        where T : IBackgroundTask
    {
        public T Task { get; private set; }
        public Exception Exception { get; private set; }

        public TaskEventArgs(T task)
        {
            Task = task;
        }

        public TaskEventArgs(T task, Exception exception)
        {
            Task = task;
            Exception = exception;
        }
    }
}