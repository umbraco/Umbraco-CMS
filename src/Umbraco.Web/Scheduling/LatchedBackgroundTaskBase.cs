using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    internal abstract class LatchedBackgroundTaskBase : ILatchedBackgroundTask
    {
        private readonly ManualResetEventSlim _latch;
        private bool _disposed;

        protected LatchedBackgroundTaskBase()
        {
            _latch = new ManualResetEventSlim(false);
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

        public WaitHandle Latch
        {
            get { return _latch.WaitHandle; }
        }

        public bool IsLatched
        {
            get { return _latch.IsSet == false; }
        }

        protected void Release()
        {
            _latch.Set();
        }

        protected void Reset()
        {
            _latch.Reset();
        }

        public abstract bool RunsOnShutdown { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // the task is going to be disposed again after execution,
        // unless it is latched again, thus indicating it wants to
        // remain active

        protected virtual void Dispose(bool disposing)
        {
            // lock on _latch instead of creating a new object as _timer is
            // private, non-null, readonly - so safe here
            lock (_latch)
            {
                if (_disposed) return;
                _disposed = true;

                _latch.Dispose();                
            }
        }
    }
}
