using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <inheritdoc />
public class DistributedJobService : IDistributedJobService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IDistributedJobRepository _distributedJobRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedJobService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider"></param>
    /// <param name="distributedJobRepository"></param>
    public DistributedJobService(ICoreScopeProvider coreScopeProvider, IDistributedJobRepository distributedJobRepository)
    {
        _coreScopeProvider = coreScopeProvider;
        _distributedJobRepository = distributedJobRepository;
    }

    /// <inheritdoc />
    public async Task<string?> TryTakeRunnableJobAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);

        var jobName = _distributedJobRepository.GetRunnableJob();

        scope.Complete();

        return jobName;
    }

    public async Task FinishJobAsync(string jobName)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);
        _distributedJobRepository.FinishJob(jobName);

        scope.Complete();
    }
}
