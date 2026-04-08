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
    /// <param name="jobName">The name of the distributed background job.</param>
    Task<DistributedBackgroundJobModel?> GetByNameAsync(string jobName);

    /// <summary>
    /// Gets all jobs.
    /// </summary>
    Task<IEnumerable<DistributedBackgroundJobModel>> GetAllAsync();

    /// <summary>
    /// Updates the specified distributed background job in the repository.
    /// </summary>
    /// <param name="distributedBackgroundJob">The distributed background job to update.</param>
    Task UpdateAsync(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Adds a job.
    /// </summary>
    /// <param name="distributedBackgroundJob">The distributed background job to add.</param>

    Task AddAsync(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Deletes the specified distributed background job from the repository.
    /// </summary>
    /// <param name="distributedBackgroundJob">The <see cref="DistributedBackgroundJobModel"/> instance representing the job to delete.</param>
    Task DeleteAsync(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Adds multiple jobs in a single batch operation.
    /// </summary>
    /// <param name="jobs">The jobs to add.</param>
    Task AddAsync(IEnumerable<DistributedBackgroundJobModel> jobs);

    /// <summary>
    /// Deletes multiple jobs in a single batch operation.
    /// </summary>
    /// <param name="jobs">The jobs to delete.</param>
    Task DeleteAsync(IEnumerable<DistributedBackgroundJobModel> jobs);
}
