using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.Services;

/// <summary>
/// Service for managing distributed jobs
/// </summary>
public interface IDistributedJobService
{
    /// <summary>
    /// Try taking a runnable job, this means locking the table, getting a runnable job, and setting its status to running.
    /// If there are no runnable job, the string will be null
    /// </summary>
    /// <returns>The name of the runnable job.</returns>
    Task<IDistributedBackgroundJob?> TryTakeRunnableAsync();

    /// <summary>
    /// Finishes a job.
    /// </summary>
    /// <param name="jobName"></param>
    /// <returns></returns>
    Task FinishAsync(string jobName);

    /// <summary>
    /// Will get all jobs and update them IF the period has changed as users may change this via config.
    /// </summary>
    /// <returns></returns>
    Task UpdateAllChangedAsync();
}
