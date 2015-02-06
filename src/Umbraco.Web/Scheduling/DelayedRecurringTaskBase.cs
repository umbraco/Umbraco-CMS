using System;
using System.Threading;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides a base class for recurring background tasks.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    internal abstract class DelayedRecurringTaskBase<T> : RecurringTaskBase<T>, IDelayedBackgroundTask
        where T : class, IBackgroundTask
    {
        private readonly int _delayMilliseconds;
        private ManualResetEvent _gate;
        private Timer _timer;

        protected DelayedRecurringTaskBase(IBackgroundTaskRunner<T> runner, int delayMilliseconds, int periodMilliseconds)
            : base(runner, periodMilliseconds)
        {
            _delayMilliseconds = delayMilliseconds;
        }

        protected DelayedRecurringTaskBase(DelayedRecurringTaskBase<T> source)
            : base(source)
        {
            _delayMilliseconds = 0;
        }

        public WaitHandle DelayWaitHandle
        {
            get
            {
                if (_delayMilliseconds == 0) return new ManualResetEvent(true);

                if (_gate != null) return _gate;
                _gate = new ManualResetEvent(false);

                // note
                // must use the single-parameter constructor on Timer to avoid it from being GC'd
                // read http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer

                _timer = new Timer(_ =>
                {
                    _timer.Dispose();
                    _timer = null;
                    _gate.Set();
                });
                _timer.Change(_delayMilliseconds, 0);
                return _gate;
            }
        }

        public bool IsDelayed
        {
            get { return _delayMilliseconds > 0; }
        }
    }
}
