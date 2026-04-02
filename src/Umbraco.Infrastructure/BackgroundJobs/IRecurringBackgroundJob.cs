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
    TimeSpan Period { get; }

    /// <summary>
    /// Timespan representing the initial delay after application start-up before the first run of the task occurs.
    /// </summary>
    /// <value>
    /// The delay.
    /// </value>
    TimeSpan Delay => RecurringBackgroundJobBase.DefaultDelay; // TODO (V19): Remove the default implementation

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
    event EventHandler PeriodChanged;

    /// <summary>
    /// Runs the background job.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    [Obsolete("Use RunJobAsync(CancellationToken) instead. This method will be removed in Umbraco 19.")]
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
