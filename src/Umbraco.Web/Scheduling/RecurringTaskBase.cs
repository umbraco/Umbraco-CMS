using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Provides a base class for recurring background tasks.
    /// </summary>
    /// <typeparam name="T">The type of the managed tasks.</typeparam>
    internal abstract class RecurringTaskBase<T> : IBackgroundTask
        where T : class, IBackgroundTask
    {
        private readonly IBackgroundTaskRunner<T> _runner;
        private readonly int _periodMilliseconds;
        private Timer _timer;
        private T _recurrent;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTaskBase{T}"/> class with a tasks runner and a period.
        /// </summary>
        /// <param name="runner">The task runner.</param>
        /// <param name="periodMilliseconds">The period.</param>
        /// <remarks>The task will repeat itself periodically. Use this constructor to create a new task.</remarks>
        protected RecurringTaskBase(IBackgroundTaskRunner<T> runner, int periodMilliseconds)
        {
            _runner = runner;
            _periodMilliseconds = periodMilliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTaskBase{T}"/> class with a source task.
        /// </summary>
        /// <param name="source">The source task.</param>
        /// <remarks>Use this constructor to create a new task from a source task in <c>GetRecurring</c>.</remarks>
        protected RecurringTaskBase(RecurringTaskBase<T> source)
        {
            _runner = source._runner;
            _timer = source._timer;
            _periodMilliseconds = source._periodMilliseconds;
        }

        /// <summary>
        /// Implements IBackgroundTask.Run().
        /// </summary>
        /// <remarks>Classes inheriting from <c>RecurringTaskBase</c> must implement <c>PerformRun</c>.</remarks>
        public virtual void Run()
        {
            PerformRun();
            Repeat();
        }

        /// <summary>
        /// Implements IBackgroundTask.RunAsync().
        /// </summary>
        /// <remarks>Classes inheriting from <c>RecurringTaskBase</c> must implement <c>PerformRun</c>.</remarks>
        public virtual async Task RunAsync(CancellationToken token)
        {
            await PerformRunAsync();
            Repeat();
        }

        private void Repeat()
        {
            // again?
            if (_runner.IsCompleted) return; // fail fast

            if (_periodMilliseconds == 0) return;

            _recurrent = GetRecurring();
            if (_recurrent == null)
            {
                _timer.Dispose();
                _timer = null;
                return; // done
            }

            // note
            // must use the single-parameter constructor on Timer to avoid it from being GC'd
            // read http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer

            _timer = _timer ?? new Timer(_ => _runner.TryAdd(_recurrent));
            _timer.Change(_periodMilliseconds, 0);
        }

        /// <summary>
        /// Indicates whether the background task can run asynchronously.
        /// </summary>
        public abstract bool IsAsync { get; }

        /// <summary>
        /// Runs the background task.
        /// </summary>
        public abstract void PerformRun();

        /// <summary>
        /// Runs the task asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> instance representing the execution of the background task.</returns>
        public abstract Task PerformRunAsync();

        /// <summary>
        /// Gets a new occurence of the recurring task.
        /// </summary>
        /// <returns>A new task instance to be queued, or <c>null</c> to terminate the recurring task.</returns>
        /// <remarks>The new task instance must be created via the <c>RecurringTaskBase(RecurringTaskBase{T} source)</c> constructor,
        /// where <c>source</c> is the current task, eg: <c>return new MyTask(this);</c></remarks>
        protected abstract T GetRecurring();

        /// <summary>
        /// Dispose the task.
        /// </summary>
        public virtual void Dispose()
        { }
    }
}