namespace Umbraco.Cms.Infrastructure.Models;

/// <summary>
/// Model for distributed background jobs.
/// </summary>
public class DistributedBackgroundJobModel
{
    /// <summary>
    /// Name of job.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Period of job.
    /// </summary>
    public TimeSpan Period { get; set; }

    /// <summary>
    /// Time of last run.
    /// </summary>
    public DateTime LastRun { get; set; }

    /// <summary>
    /// If the job is running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Time of last attempted run.
    /// </summary>
    public DateTime LastAttemptedRun { get; set; }
}
