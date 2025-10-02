namespace Umbraco.Cms.Infrastructure.Persistence.Repositories;

/// <summary>
///   Defines a repository for managing distributed jobs.
/// </summary>
public interface IDistributedJobRepository
{
    /// <summary>
    /// Gets the name of the next runnable job from the database, if there are no jobs to run, get a null value.
    /// </summary>
    /// <returns></returns>
    string? GetRunnableJob();

    /// <summary>
    /// Finishes the job by marking the IsRunning column false and updating LastRun and LastAttemptedRun columns.
    /// </summary>
    /// <param name="jobName"></param>
    void FinishJob(string jobName);
}
