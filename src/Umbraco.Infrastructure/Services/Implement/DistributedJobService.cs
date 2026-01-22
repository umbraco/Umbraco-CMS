using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly DistributedJobSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedJobService"/> class.
    /// </summary>
    [Obsolete("Use the constructor that accepts IOptions<DistributedJobSettings>. Scheduled for removal in V18.")]
    public DistributedJobService(
        ICoreScopeProvider coreScopeProvider,
        IDistributedJobRepository distributedJobRepository,
        IEnumerable<IDistributedBackgroundJob> distributedBackgroundJobs,
        ILogger<DistributedJobService> logger)
        : this(
            coreScopeProvider,
            distributedJobRepository,
            distributedBackgroundJobs,
            logger,
            StaticServiceProvider.Instance.GetRequiredService<IOptions<DistributedJobSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedJobService"/> class.
    /// </summary>
    public DistributedJobService(
        ICoreScopeProvider coreScopeProvider,
        IDistributedJobRepository distributedJobRepository,
        IEnumerable<IDistributedBackgroundJob> distributedBackgroundJobs,
        ILogger<DistributedJobService> logger,
        IOptions<DistributedJobSettings> settings)
    {
        _coreScopeProvider = coreScopeProvider;
        _distributedJobRepository = distributedJobRepository;
        _distributedBackgroundJobs = distributedBackgroundJobs;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public async Task<IDistributedBackgroundJob?> TryTakeRunnableAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);

        IEnumerable<DistributedBackgroundJobModel> jobs = _distributedJobRepository.GetAll();
        DistributedBackgroundJobModel? job = jobs.FirstOrDefault(x => x.LastRun < DateTime.UtcNow - x.Period
                                                                      && (x.IsRunning is false || x.LastAttemptedRun < DateTime.UtcNow - x.Period - _settings.MaximumExecutionTime));

        if (job is null)
        {
            // No runnable jobs for now.
            scope.Complete();
            return null;
        }

        job.LastAttemptedRun = DateTime.UtcNow;
        job.IsRunning = true;
        _distributedJobRepository.Update(job);
        scope.Complete();

        IDistributedBackgroundJob? distributedJob = _distributedBackgroundJobs.FirstOrDefault(x => x.Name == job.Name);

        if (distributedJob is null)
        {
            _logger.LogWarning("Could not find a distributed job with the name '{JobName}'", job.Name);
        }
        else
        {
            _logger.LogDebug("Running distributed job with the name '{JobName}'", job.Name);
        }

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
        _logger.LogInformation("Registering distributed background jobs");

        // Pre-compute registered job data outside the lock to minimize lock hold time
        var registeredJobsByName = _distributedBackgroundJobs.ToDictionary(x => x.Name, x => x.Period);

        // Early exit if no registered jobs
        if (registeredJobsByName.Count is 0)
        {
            _logger.LogInformation("No distributed background jobs to register");
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.DistributedJobs);

        DistributedBackgroundJobModel[] existingJobs = _distributedJobRepository.GetAll().ToArray();
        var existingJobsByName = existingJobs.ToDictionary(x => x.Name);

        // Collect all changes first, then execute - minimizes time spent in the critical section
        var jobsToAdd = new List<DistributedBackgroundJobModel>();
        DateTime utcNow = DateTime.UtcNow;

        foreach (KeyValuePair<string, TimeSpan> registeredJob in registeredJobsByName)
        {
            if (existingJobsByName.TryGetValue(registeredJob.Key, out DistributedBackgroundJobModel? existingJob))
            {
                // Update only if period has actually changed
                if (existingJob.Period != registeredJob.Value)
                {
                    existingJob.Period = registeredJob.Value;
                    _distributedJobRepository.Update(existingJob);
                }
            }
            else
            {
                // Collect new jobs for batch insert
                jobsToAdd.Add(new DistributedBackgroundJobModel
                {
                    Name = registeredJob.Key,
                    Period = registeredJob.Value,
                    LastRun = utcNow,
                    IsRunning = false,
                    LastAttemptedRun = utcNow,
                });
            }

            _logger.LogInformation("Registered distributed background job {JobName}, running every {Period}", registeredJob.Key, registeredJob.Value);
        }

        // Batch insert new jobs
        if (jobsToAdd.Count > 0)
        {
            _distributedJobRepository.Add(jobsToAdd);
        }

        // Batch delete jobs that are no longer registered
        var jobsToRemove = existingJobs
            .Where(x => registeredJobsByName.ContainsKey(x.Name) is false)
            .ToList();

        if (jobsToRemove.Count > 0)
        {
            _distributedJobRepository.Delete(jobsToRemove);
        }

        scope.Complete();

        _logger.LogInformation("Completed registering distributed background jobs");
    }
}
