using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Web.Scheduling
{
    internal abstract class LatchedBackgroundTaskBase : DisposableObject, ILatchedBackgroundTask
    {
        private TaskCompletionSource<bool> _latch;

        protected LatchedBackgroundTaskBase()
        {
            _latch = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Implements IBackgroundTask.Run().
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Implements IBackgroundTask.RunAsync().
        /// </summary>
        public abstract Task RunAsync(CancellationToken token);

        /// <summary>
        /// Indicates whether the background task can run asynchronously.
        /// </summary>
        public abstract bool IsAsync { get; }

        public Task Latch
        {
            get { return _latch.Task; }
        }

        public bool IsLatched
        {
            get { return _latch.Task.IsCompleted == false; }
        }

        protected void Release()
        {
            _latch.SetResult(true);
        }

        protected void Reset()
        {
            _latch = new TaskCompletionSource<bool>();
        }

        public abstract bool RunsOnShutdown { get; }

        // the task is going to be disposed after execution,
        // unless it is latched again, thus indicating it wants to
        // remain active

        protected override void DisposeResources()
        { }
    }
}
