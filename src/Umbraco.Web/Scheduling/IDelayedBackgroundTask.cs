using System.Threading;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Represents a delayed background task.
    /// </summary>
    /// <remarks>Delayed background tasks can suspend their execution until
    /// a condition is met. However if the tasks runner has to terminate,
    /// delayed background tasks are executed immediately.</remarks>
    internal interface IDelayedBackgroundTask : IBackgroundTask
    {
        /// <summary>
        /// Gets a wait handle on the task condition.
        /// </summary>
        WaitHandle DelayWaitHandle { get; }

        /// <summary>
        /// Gets a value indicating whether the task is delayed.
        /// </summary>
        bool IsDelayed { get; }
    }
}
