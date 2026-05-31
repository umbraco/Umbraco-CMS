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
    /// Marks the specified distributed job as finished.
    /// </summary>
    /// <param name="jobName">The name of the job to finish.</param>
    /// <returns>A task that represents the asynchronous finish operation.</returns>
    Task FinishAsync(string jobName);

    /// <summary>
    /// Ensures all distributed jobs are registered in the database on startup.
    /// </summary>
    /// <remarks>
    /// This method handles two scenarios:
    /// <list type="bullet">
    /// <item><description>Fresh install: Adds all registered jobs to the database</description></item>
    /// <item><description>Restart: Updates existing jobs where periods have changed and adds any new jobs</description></item>
    /// </list>
    /// Jobs that exist in the database but are no longer registered in code will be removed.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task EnsureJobsAsync();
}
