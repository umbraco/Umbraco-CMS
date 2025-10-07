using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Services;

/// <summary>
/// Service for managing distributed jobs.
/// </summary>
public interface IDistributedJobService
{
    /// <summary>
    /// Attempts to claim a runnable distributed job for execution.
    /// </summary>
    /// <returns>
    /// The claimed <see cref="IDistributedBackgroundJob"/> if available, or <see langword="null"/> if no jobs are ready to run.
    /// </returns>
    Task<IDistributedBackgroundJob?> TryTakeRunnableAsync();

    /// <summary>
    /// Finishes a job.
    /// </summary>
    Task FinishAsync(string jobName);

    /// <summary>
    /// Will get all jobs and update them IF the period has changed as users may change this via config.
    /// </summary>
    Task UpdateAllChangedAsync();
}
