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
    /// Updates a job.
    /// </summary>
    void Update(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Adds a job.
    /// </summary>
    void Add(DistributedBackgroundJobModel distributedBackgroundJob);

    /// <summary>
    /// Deletes a job.
    /// </summary>
    void Delete(DistributedBackgroundJobModel distributedBackgroundJob);
}
