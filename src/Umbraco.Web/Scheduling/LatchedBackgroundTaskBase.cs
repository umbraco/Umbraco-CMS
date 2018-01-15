using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides a base class for latched background tasks.
    /// </summary>
    /// <remarks>Implement by overriding Run or RunAsync and then IsAsync accordingly,
    /// depending on whether the task is implemented as a sync or async method, and then
    /// optionnally overriding RunsOnShutdown, to indicate whether the latched task should run
    /// immediately on shutdown, or just be abandonned (default).</remarks>
    public abstract class LatchedBackgroundTaskBase : DisposableObjectSlim, ILatchedBackgroundTask
    {
        private TaskCompletionSource<bool> _latch;

        protected LatchedBackgroundTaskBase()
        {
            _latch = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Implements IBackgroundTask.Run().
        /// </summary>
        public virtual void Run()
        {
            throw new NotSupportedException("This task cannot run synchronously.");
        }

        /// <summary>
        /// Implements IBackgroundTask.RunAsync().
        /// </summary>
        public virtual Task RunAsync(CancellationToken token)
        {
            throw new NotSupportedException("This task cannot run asynchronously.");
        }

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

        public virtual bool RunsOnShutdown { get { return false; } }

        // the task is going to be disposed after execution,
        // unless it is latched again, thus indicating it wants to
        // remain active

        protected override void DisposeResources()
        { }
    }
}
