using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

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
    /// Updates all jobs.
    /// </summary>
    void Update(DistributedBackgroundJobModel distributedBackgroundJob);
}
