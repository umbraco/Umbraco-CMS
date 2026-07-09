using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories;

/// <summary>
///   Defines a repository for managing distributed jobs.
/// </summary>
public interface IDistributedJobRepository
{
    /// <summary>
    /// Gets a job by name.
    /// </summary>
    /// <returns></returns>
    DistributedBackgroundJobModel? GetByName(string jobName);

    /// <summary>
    /// Gets all jobs.
    /// </summary>
    /// <returns></returns>
    IEnumerable<DistributedBackgroundJobModel> GetAll();

    /// <summary>
    /// Updates the specified distributed background job in the repository.
    /// </summary>
    /// <param name="distributedBackgroundJob">The distributed background job to update.</param>
    void Update(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Adds a job.
    /// </summary>
    void Add(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Deletes the specified distributed background job from the repository.
    /// </summary>
    /// <param name="distributedBackgroundJob">The <see cref="DistributedBackgroundJobModel"/> instance representing the job to delete.</param>
    void Delete(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Adds multiple jobs in a single batch operation.
    /// </summary>
    /// <param name="jobs">The jobs to add.</param>
    void Add(IEnumerable<DistributedBackgroundJobModel> jobs)
    {
        // TODO: Delete default implementation in V18
        foreach (DistributedBackgroundJobModel job in jobs)
        {
            Add(job);
        }
    }

    /// <summary>
    /// Deletes multiple jobs in a single batch operation.
    /// </summary>
    /// <param name="jobs">The jobs to delete.</param>
    void Delete(IEnumerable<DistributedBackgroundJobModel> jobs)
    {
        // TODO: Delete default implementation in V18
        foreach (DistributedBackgroundJobModel job in jobs)
        {
            Delete(job);
        }
    }
}
