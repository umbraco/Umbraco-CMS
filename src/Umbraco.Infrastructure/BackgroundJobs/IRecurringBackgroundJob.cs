using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;
/// <summary>
///     A recurring background job
/// </summary>
public interface IRecurringBackgroundJob
{
    static readonly TimeSpan DefaultDelay = System.TimeSpan.FromMinutes(3);
    static readonly ServerRole[] DefaultServerRoles = new[] { ServerRole.Single, ServerRole.SchedulingPublisher };

    /// <summary>
    /// Timespan representing how often the task should recur.
    /// </summary>
    TimeSpan Period { get; }

    /// <summary>
    /// Timespan representing the initial delay after application start-up before the first run of the task
    /// occurs.
    /// </summary>
    TimeSpan Delay { get => DefaultDelay; }

    ServerRole[] ServerRoles { get => DefaultServerRoles; }

    event EventHandler PeriodChanged;

    Task RunJobAsync();
}

