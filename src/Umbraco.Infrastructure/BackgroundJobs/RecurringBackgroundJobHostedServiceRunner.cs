using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
/// A hosted service that discovers and starts hosted services for any recurring background jobs in the DI container.
/// </summary>
public class RecurringBackgroundJobHostedServiceRunner : IHostedService
{
    private readonly ILogger<RecurringBackgroundJobHostedServiceRunner> _logger;
    private readonly List<IRecurringBackgroundJob> _jobs;
    private readonly Func<IRecurringBackgroundJob, IHostedService> _jobFactory;
    private readonly ConcurrentDictionary<Type, IHostedService> _hostedServices = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobHostedServiceRunner" /> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger{RecurringBackgroundJobHostedServiceRunner}" /> used for logging within the runner.</param>
    /// <param name="jobs">A collection of <see cref="IRecurringBackgroundJob" /> instances to be managed by the runner.</param>
    /// <param name="jobFactory">A factory function that creates an <see cref="IHostedService" /> for each <see cref="IRecurringBackgroundJob" />.</param>
    public RecurringBackgroundJobHostedServiceRunner(
        ILogger<RecurringBackgroundJobHostedServiceRunner> logger,
        IEnumerable<IRecurringBackgroundJob> jobs,
        Func<IRecurringBackgroundJob, IHostedService> jobFactory)
    {
        _jobs = jobs.ToList();
        _logger = logger;
        _jobFactory = jobFactory;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting recurring background jobs hosted services");

        foreach (IRecurringBackgroundJob job in _jobs)
        {
            Type jobType = job.GetType();
            var added = false;

            try
            {
                IHostedService hostedService = _hostedServices.GetOrAdd(jobType, _ =>
                {
                    _logger.LogDebug("Creating background hosted service for {JobTypeName}", jobType.Name);

                    IHostedService hostedService = _jobFactory(job);
                    added = true;

                    return hostedService;
                });

                if (!added)
                {
                    _logger.LogWarning("A background hosted service for {JobTypeName} is already registered, skipping duplicate", jobType.Name);
                    continue;
                }

                _logger.LogInformation("Starting a background hosted service for {JobTypeName} with a delay of {Delay}, running every {Period}", jobType.Name, job.Delay, job.Period);

                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (added)
                {
                    // Ensure we don't stop hosted services that were not successfully started
                    _hostedServices.TryRemove(jobType, out _);
                }

                _logger.LogError(ex, "Failed to start background hosted service for {JobTypeName}", jobType.Name);
            }
        }

        _logger.LogInformation("Completed starting recurring background jobs hosted services");
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping recurring background jobs hosted services");

        foreach (Type jobType in _hostedServices.Keys)
        {
            if (_hostedServices.TryRemove(jobType, out IHostedService? hostedService))
            {
                try
                {
                    _logger.LogInformation("Stopping background hosted service for {JobTypeName}", jobType.Name);

                    await hostedService.StopAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to stop background hosted service for {JobTypeName}", jobType.Name);
                }
            }
        }

        _logger.LogInformation("Completed stopping recurring background jobs hosted services");
    }

    /// <summary>
    /// Signals the background loop for the specified job type to execute immediately.
    /// After the triggered execution, the original schedule is kept.
    /// </summary>
    /// <typeparam name="TJob">The type of the recurring background job to trigger.</typeparam>
    /// <returns>
    ///   <c>true</c> if the job was found and triggered; <c>false</c> if no hosted service is running for this job type.
    /// </returns>
    public bool TriggerExecution<TJob>()
        where TJob : IRecurringBackgroundJob
        => TriggerExecution<TJob>(NextExecutionStrategy.None);

    /// <summary>
    /// Signals the background loop for the specified job type to execute immediately, with the specified strategy for determining the next execution after the triggered one completes.
    /// </summary>
    /// <typeparam name="TJob">The type of the recurring background job to trigger.</typeparam>
    /// <param name="strategy">Controls the delay after the triggered execution.</param>
    /// <returns>
    ///   <c>true</c> if the job was found and triggered; <c>false</c> if no hosted service is running for this job type.
    /// </returns>
    public bool TriggerExecution<TJob>(NextExecutionStrategy strategy)
        where TJob : IRecurringBackgroundJob
    {
        if (FindHostedService<TJob>() is not { } hostedService)
        {
            return false;
        }

        hostedService.TriggerExecution(strategy);

        return true;
    }

    /// <summary>
    /// Signals the background loop for the specified job type to execute immediately.
    /// After the triggered execution, the next execution is scheduled after the specified delay (measured from execution start; execution time is subtracted to prevent drift).
    /// </summary>
    /// <typeparam name="TJob">The type of the recurring background job to trigger.</typeparam>
    /// <param name="nextDelay">The target interval from execution start to the next execution. Execution time is subtracted to prevent drift.</param>
    /// <returns>
    ///   <c>true</c> if the job was found and triggered; <c>false</c> if no hosted service is running for this job type.
    /// </returns>
    public bool TriggerExecution<TJob>(TimeSpan nextDelay)
        where TJob : IRecurringBackgroundJob
    {
        if (FindHostedService<TJob>() is not { } hostedService)
        {
            return false;
        }

        hostedService.TriggerExecution(nextDelay);

        return true;
    }

    private RecurringHostedServiceBase? FindHostedService<TJob>()
        where TJob : IRecurringBackgroundJob
        => _hostedServices.TryGetValue(typeof(TJob), out IHostedService? service) ? service as RecurringHostedServiceBase : null;
}
