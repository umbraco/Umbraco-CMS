using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides a base class for recurring background tasks.
    /// </summary>
    internal abstract class RecurringTaskBase : LatchedBackgroundTaskBase
    {
        private readonly IBackgroundTaskRunner<RecurringTaskBase> _runner;
        private readonly int _periodMilliseconds;
        private readonly Timer _timer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTaskBase"/> class.
        /// </summary>
        /// <param name="runner">The task runner.</param>
        /// <param name="delayMilliseconds">The delay.</param>
        /// <param name="periodMilliseconds">The period.</param>
        /// <remarks>The task will repeat itself periodically. Use this constructor to create a new task.</remarks>
        protected RecurringTaskBase(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds)
        {
            _runner = runner;
            _periodMilliseconds = periodMilliseconds;

            // note
            // must use the single-parameter constructor on Timer to avoid it from being GC'd
            // read http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer

            _timer = new Timer(_ => Release());
            _timer.Change(delayMilliseconds, 0);
        }

        /// <summary>
        /// Implements IBackgroundTask.Run().
        /// </summary>
        /// <remarks>Classes inheriting from <c>RecurringTaskBase</c> must implement <c>PerformRun</c>.</remarks>
        public override void Run()
        {
            var shouldRepeat = PerformRun();
            if (shouldRepeat) Repeat();
        }

        /// <summary>
        /// Implements IBackgroundTask.RunAsync().
        /// </summary>
        /// <remarks>Classes inheriting from <c>RecurringTaskBase</c> must implement <c>PerformRun</c>.</remarks>
        public override async Task RunAsync(CancellationToken token)
        {
            var shouldRepeat = await PerformRunAsync(token);
            if (shouldRepeat) Repeat();
        }

        private void Repeat()
        {
            // again?
            if (_runner.IsCompleted) return; // fail fast

            if (_periodMilliseconds == 0) return; // safe

            Reset(); // re-latch

            // try to add again (may fail if runner has completed)
            // if added, re-start the timer, else kill it
            if (_runner.TryAdd(this))
                _timer.Change(_periodMilliseconds, 0);
            else
                Dispose(true);
        }

        /// <summary>
        /// Runs the background task.
        /// </summary>
        /// <returns>A value indicating whether to repeat the task.</returns>
        public abstract bool PerformRun();

        /// <summary>
        /// Runs the task asynchronously.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A <see cref="Task{T}"/> instance representing the execution of the background task,
        /// and returning a value indicating whether to repeat the task.</returns>
        public abstract Task<bool> PerformRunAsync(CancellationToken token);

        protected override void DisposeResources()
        {
            base.DisposeResources();

            // stop the timer
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }
    }
}