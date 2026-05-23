using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// A recurring background job.
/// </summary>
public interface IRecurringBackgroundJob
{
    /// <summary>
    /// The default delay to use for recurring tasks for the first run after application start-up if no alternative is configured.
    /// </summary>
    [Obsolete("Use RecurringBackgroundJobBase.DefaultDelay instead. Scheduled for removal in Umbraco 19.")]
    static readonly TimeSpan DefaultDelay = RecurringBackgroundJobBase.DefaultDelay;

    /// <summary>
    /// The default server roles that recurring background jobs run on.
    /// </summary>
    [Obsolete("Use RecurringBackgroundJobBase.DefaultServerRoles instead. Scheduled for removal in Umbraco 19.")]
    static readonly ServerRole[] DefaultServerRoles = RecurringBackgroundJobBase.DefaultServerRoles;

    /// <summary>
    /// Timespan representing how often the task should recur.
    /// </summary>
    /// <value>
    /// The period.
    /// </value>
    /// <remarks>
    /// Set to <see cref="Timeout.InfiniteTimeSpan" /> to (temporarily) disable automatic scheduling and turn the job into a manually triggered one (via <see cref="IRecurringBackgroundJobTrigger{TJob}" />). Raise <see cref="PeriodChanged" /> to switch back to a finite period at runtime.
    /// </remarks>
    TimeSpan Period { get; }

    /// <summary>
    /// Timespan representing the initial delay after application start-up before the first run of the task occurs.
    /// </summary>
    /// <value>
    /// The delay.
    /// </value>
    /// <remarks>
    /// Set to <see cref="Timeout.InfiniteTimeSpan" /> to skip the automatic first run entirely; the first execution then only occurs when manually triggered via <see cref="IRecurringBackgroundJobTrigger{TJob}" />.
    /// </remarks>
    TimeSpan Delay => RecurringBackgroundJobBase.DefaultDelay; // TODO (V19): Remove the default implementation

    /// <summary>
    /// Timespan to wait before re-evaluating execution conditions when an execution is ignored (e.g. runtime not ready, wrong server role or not main domain).
    /// </summary>
    /// <value>
    /// The ignored delay.
    /// </value>
    /// <remarks>
    /// This back-off prevents tight looping when <see cref="Period" /> is short (or <see cref="TimeSpan.Zero" />) and an execution is skipped without invoking <see cref="RunJobAsync(CancellationToken)" />.
    /// Set to <see cref="Timeout.InfiniteTimeSpan" /> to disable the job for the remaining application lifecycle once an ignored condition is encountered — useful when the condition is known not to change (e.g. a server role that will not be promoted on this instance).
    /// </remarks>
    TimeSpan IgnoredDelay => RecurringBackgroundJobBase.DefaultIgnoredDelay; // TODO (V19): Remove the default implementation

    /// <summary>
    /// Gets the server roles the task executes on.
    /// </summary>
    /// <value>
    /// The server roles.
    /// </value>
    ServerRole[] ServerRoles => RecurringBackgroundJobBase.DefaultServerRoles; // TODO (V19): Remove the default implementation

    /// <summary>
    /// This event should be raised when the <see cref="Period" /> property changes to notify the background job manager to update the schedule for this job.
    /// </summary>
    event EventHandler PeriodChanged; // TODO (V19): Change to `event EventHandler? PeriodChanged;` so implementations can use field-like event syntax without manual backing-delegate accessors.

    /// <summary>
    /// This event should be raised when the <see cref="IgnoredDelay" /> property changes (e.g. from <see cref="Timeout.InfiniteTimeSpan" /> back to a finite value) to interrupt any in-progress ignored back-off and re-read the new value.
    /// </summary>
    event EventHandler IgnoredDelayChanged
    {
        add { }
        remove { }
    } // TODO (V19): Remove the default implementation and change to `event EventHandler? IgnoredDelayChanged;` so implementations can use field-like event syntax without manual backing-delegate accessors.

    /// <summary>
    /// Runs the background job.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    [Obsolete("Use RunJobAsync(CancellationToken) instead. Scheduled for removal in Umbraco 19.")]
    Task RunJobAsync();

    /// <summary>
    /// Runs the background job with cancellation support.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task RunJobAsync(CancellationToken cancellationToken)
#pragma warning disable CS0618 // Type or member is obsolete
        => RunJobAsync(); // TODO (V19): Remove the default implementation when RunJobAsync() is removed
#pragma warning restore CS0618 // Type or member is obsolete
}
