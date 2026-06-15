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

    // Captured once: which jobs align to the clock is a startup configuration concern (changing it requires a restart),
    // so there's no need to re-evaluate it on every poll.
    private readonly Lazy<HashSet<string>> _clockAlignedJobNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedJobService"/> class.
    /// </summary>
    [Obsolete("Use the constructor that accepts IOptions<DistributedJobSettings>. Scheduled for removal in Umbraco 18.")]
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
    /// <param name="coreScopeProvider">Provides access to the core scope for database operations.</param>
    /// <param name="distributedJobRepository">Repository for managing distributed jobs.</param>
    /// <param name="distributedBackgroundJobs">A collection of distributed background job implementations.</param>
    /// <param name="logger">The logger used for logging job service operations.</param>
    /// <param name="settings">The configuration settings for distributed jobs.</param>
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
        _clockAlignedJobNames = new Lazy<HashSet<string>>(() =>
            _distributedBackgroundJobs.Where(x => x.AlignToClock).Select(x => x.Name).ToHashSet());
    }

    /// <inheritdoc />
    public async Task<IDistributedBackgroundJob?> TryTakeRunnableAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        scope.EagerWriteLock(Constants.Locks.DistributedJobs);

        DateTime utcNow = DateTime.UtcNow;
        HashSet<string> clockAlignedJobNames = _clockAlignedJobNames.Value;

        IEnumerable<DistributedBackgroundJobModel> jobs = _distributedJobRepository.GetAll();
        DistributedBackgroundJobModel? job = jobs.FirstOrDefault(x =>
            IsDue(x, utcNow, clockAlignedJobNames.Contains(x.Name))
            && (x.IsRunning is false || x.LastAttemptedRun < utcNow - x.Period - _settings.MaximumExecutionTime));

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

    /// <summary>
    ///     Determines whether a job is due to run.
    /// </summary>
    /// <param name="job">The job state.</param>
    /// <param name="utcNow">The current UTC time.</param>
    /// <param name="aligned">
    ///     Whether the job's runs are aligned to clock boundaries (see <see cref="IDistributedBackgroundJob.AlignToClock" />).
    /// </param>
    /// <remarks>
    ///     For non-aligned jobs the period counts from the previous run's completion (<c>LastRun + Period</c>, drifting).
    ///     For aligned jobs the job is due once a clock boundary — a multiple of the period anchored at the UTC epoch —
    ///     has fallen strictly after the previous run's completion. Boundaries are anchored to UTC, not the server's
    ///     local time zone. This is overrun-safe: if a run takes longer than the period, the boundary it would have
    ///     targeted has already passed, so the missed boundary is skipped rather than triggering back-to-back runs.
    /// </remarks>
    internal static bool IsDue(DistributedBackgroundJobModel job, DateTime utcNow, bool aligned)
    {
        if (aligned == false || job.Period <= TimeSpan.Zero)
        {
            return job.LastRun < utcNow - job.Period;
        }

        long periodTicks = job.Period.Ticks;

        // Largest UTC clock boundary (a multiple of the period, anchored at the epoch) that is <= now.
        long currentBoundaryTicks = utcNow.Ticks / periodTicks * periodTicks;

        return currentBoundaryTicks > job.LastRun.Ticks;
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

        // Clock-aligned jobs only hit their boundaries as tightly as the poll interval allows. If the poll interval
        // is longer than the job's period, boundaries between polls are silently missed.
        foreach (IDistributedBackgroundJob job in _distributedBackgroundJobs)
        {
            if (job.AlignToClock && job.Period < _settings.Period)
            {
                _logger.LogWarning(
                    "Distributed background job '{JobName}' aligns to the clock with a period of {Period}, but the distributed job poll interval is longer ({PollInterval}). Clock boundaries shorter than the poll interval will be missed; set Umbraco:CMS:DistributedJobs:Period to be no longer than the job period.",
                    job.Name,
                    job.Period,
                    _settings.Period);
            }
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
