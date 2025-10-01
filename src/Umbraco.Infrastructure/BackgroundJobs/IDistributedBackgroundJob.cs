namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

public interface IDistributedBackgroundJob
{
    static readonly TimeSpan DefaultDelay = System.TimeSpan.FromMinutes(3);

    /// <summary>
    /// Name of the job
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Timespan representing how often the task should recur.
    /// </summary>
    TimeSpan Period { get; }

    /// <summary>
    /// Timespan representing the initial delay after application start-up before the first run of the task
    /// occurs.
    /// </summary>
    TimeSpan Delay { get => DefaultDelay; }

    event EventHandler PeriodChanged;

    Task RunJobAsync();
}
