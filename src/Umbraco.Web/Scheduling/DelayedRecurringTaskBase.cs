using System;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides a base class for recurring background tasks.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    internal abstract class DelayedRecurringTaskBase<T> : RecurringTaskBase<T>, ILatchedBackgroundTask
        where T : class, IBackgroundTask
    {
        private readonly ManualResetEventSlim _latch;
        private Timer _timer;

        protected DelayedRecurringTaskBase(IBackgroundTaskRunner<T> runner, int delayMilliseconds, int periodMilliseconds)
            : base(runner, periodMilliseconds)
        {
            if (delayMilliseconds > 0)
            {
                _latch = new ManualResetEventSlim(false);
                _timer = new Timer(_ =>
                {
                    _timer.Dispose();
                    _timer = null;
                    _latch.Set();
                });
                _timer.Change(delayMilliseconds, 0);
            }
        }

        protected DelayedRecurringTaskBase(DelayedRecurringTaskBase<T> source)
            : base(source)
        {
            // no latch on recurring instances
            _latch = null;
        }

        public override void Run()
        {
            if (_latch != null)
                _latch.Dispose();
            base.Run();
        }

        public override async Task RunAsync(CancellationToken token)
        {
            if (_latch != null)
                _latch.Dispose();
            await base.RunAsync(token);
        }

        public WaitHandle Latch
        {
            get
            {
                if (_latch == null)
                    throw new InvalidOperationException("The task is not latched.");
                return _latch.WaitHandle;
            }
        }

        public bool IsLatched
        {
            get { return _latch != null; }
        }

        public virtual bool RunsOnShutdown
        {
            get { return true; }
        }
    }
}
