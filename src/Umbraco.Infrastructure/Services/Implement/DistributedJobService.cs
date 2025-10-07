using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <inheritdoc />
public class DistributedJobService : IDistributedJobService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IDistributedJobRepository _distributedJobRepository;
    private readonly IEnumerable<IDistributedBackgroundJob> _distributedBackgroundJobs;
    private readonly ILogger<DistributedJobService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedJobService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider"></param>
    /// <param name="distributedJobRepository"></param>
    /// <param name="distributedBackgroundJobs"></param>
    /// <param name="logger"></param>
    public DistributedJobService(
        ICoreScopeProvider coreScopeProvider,
        IDistributedJobRepository distributedJobRepository,
        IEnumerable<IDistributedBackgroundJob> distributedBackgroundJobs,
        ILogger<DistributedJobService> logger)
    {
        _coreScopeProvider = coreScopeProvider;
        _distributedJobRepository = distributedJobRepository;
        _distributedBackgroundJobs = distributedBackgroundJobs;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IDistributedBackgroundJob?> TryTakeRunnableAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);

        var jobName = _distributedJobRepository.GetRunnable();

        IDistributedBackgroundJob? job = _distributedBackgroundJobs.FirstOrDefault(x => x.Name == jobName);

        if (job is null)
        {
            _logger.LogWarning("Could not find a distributed job with the name '{JobName}'", jobName);
        }

        scope.Complete();

        return job;
    }

    public async Task FinishAsync(string jobName)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);
        _distributedJobRepository.Finish(jobName);

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task UpdateAllChangedAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.DistributedJobs);

        IEnumerable<DistributedBackgroundJobModel> jobs = _distributedJobRepository.GetAll();

        var updatedJobs = new List<DistributedBackgroundJobModel>();
        foreach (DistributedBackgroundJobModel backgroundJobModel in jobs)
        {
            IDistributedBackgroundJob? distributedBackgroundJob = _distributedBackgroundJobs.FirstOrDefault(x => x.Name == backgroundJobModel.Name);
            if (distributedBackgroundJob is null || distributedBackgroundJob.Period == backgroundJobModel.Period)
            {
                continue;
            }

            backgroundJobModel.Period = distributedBackgroundJob.Period;
            updatedJobs.Add(backgroundJobModel);
        }

        foreach (DistributedBackgroundJobModel updatedJob in updatedJobs)
        {
            _distributedJobRepository.Update(updatedJob);
        }

        scope.Complete();
    }
}
