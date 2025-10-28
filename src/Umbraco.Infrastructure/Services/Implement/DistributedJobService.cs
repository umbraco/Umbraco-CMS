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

        IEnumerable<DistributedBackgroundJobModel> jobs = _distributedJobRepository.GetAll();
        DistributedBackgroundJobModel? job = jobs.FirstOrDefault(x => x.LastRun < DateTime.UtcNow - x.Period);

        if (job is null)
        {
            // No runnable jobs for now.
            return null;
        }

        job.LastAttemptedRun = DateTime.UtcNow;
        job.IsRunning = true;
        _distributedJobRepository.Update(job);

        IDistributedBackgroundJob? distributedJob = _distributedBackgroundJobs.FirstOrDefault(x => x.Name == job.Name);

        if (distributedJob is null)
        {
            _logger.LogWarning("Could not find a distributed job with the name '{JobName}'", job.Name);
        }

        scope.Complete();

        return distributedJob;
    }

    /// <inheritdoc />
    public async Task FinishAsync(string jobName)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);
        DistributedBackgroundJobModel? job = _distributedJobRepository.GetByName(jobName);

        if (job is null)
        {
            _logger.LogWarning("Could not finish a distributed job with the name '{JobName}'", jobName);
            return;
        }

        DateTime currentDateTime = DateTime.UtcNow;
        job.LastAttemptedRun = currentDateTime;
        job.LastRun = currentDateTime;
        job.IsRunning = false;
        _distributedJobRepository.Update(job);

        scope.Complete();
    }


    /// <inheritdoc />
    public async Task EnsureJobsAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.DistributedJobs);

        DistributedBackgroundJobModel[] existingJobs = _distributedJobRepository.GetAll().ToArray();
        var existingJobsByName = existingJobs.ToDictionary(x => x.Name);

        foreach (IDistributedBackgroundJob registeredJob in _distributedBackgroundJobs)
        {
            if (existingJobsByName.TryGetValue(registeredJob.Name, out DistributedBackgroundJobModel? existingJob))
            {
                // Update if period has changed
                if (existingJob.Period != registeredJob.Period)
                {
                    existingJob.Period = registeredJob.Period;
                    _distributedJobRepository.Update(existingJob);
                }
            }
            else
            {
                // Add new job (fresh install or newly registered job)
                var newJob = new DistributedBackgroundJobModel
                {
                    Name = registeredJob.Name,
                    Period = registeredJob.Period,
                    LastRun = DateTime.UtcNow,
                    IsRunning = false,
                    LastAttemptedRun = DateTime.UtcNow,
                };
                _distributedJobRepository.Add(newJob);
            }
        }

        // Remove jobs that are no longer registered in code
        var registeredJobNames = _distributedBackgroundJobs.Select(x => x.Name).ToHashSet();
        IEnumerable<DistributedBackgroundJobModel> jobsToRemove = existingJobs.Where(x => registeredJobNames.Contains(x.Name) is false);

        foreach (DistributedBackgroundJobModel jobToRemove in jobsToRemove)
        {
            _distributedJobRepository.Delete(jobToRemove);
        }

        scope.Complete();
    }
}
