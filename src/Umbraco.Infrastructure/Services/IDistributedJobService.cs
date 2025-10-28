﻿using Umbraco.Cms.Infrastructure.BackgroundJobs;

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
    Task EnsureJobsAsync();
}
