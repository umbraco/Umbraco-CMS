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
    private readonly List<TypedServiceJob> _hostedServices = new();

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
            try
            {

                _logger.LogDebug("Creating background hosted service for {job}", jobType.Name);
                IHostedService hostedService = _jobFactory(job);

                _logger.LogInformation("Starting a background hosted service for {job} with a delay of {delay}, running every {period}", jobType.Name, job.Delay, job.Period);

                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);

                _hostedServices.Add(new TypedServiceJob(jobType, hostedService));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to start background hosted service for {job}", jobType.Name);
            }
        }

        _logger.LogInformation("Completed starting recurring background jobs hosted services");
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping recurring background jobs hosted services");

        foreach (TypedServiceJob typedServiceJob in _hostedServices)
        {
            try
            {
                _logger.LogInformation("Stopping background hosted service for {job}", typedServiceJob.JobType.Name);
                await typedServiceJob.HostedService.StopAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to stop background hosted service for {job}", typedServiceJob.JobType.Name);
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
    /// After the triggered execution, the loop waits for the specified delay before the next execution.
    /// </summary>
    /// <typeparam name="TJob">The type of the recurring background job to trigger.</typeparam>
    /// <param name="nextDelay">The delay to wait after the triggered execution completes (execution time is subtracted to prevent drift).</param>
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
        => _hostedServices.Find(x => x.JobType == typeof(TJob))?.HostedService as RecurringHostedServiceBase;

    private sealed class TypedServiceJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedServiceJob" /> class using the specified job type and hosted service instance.
        /// </summary>
        /// <param name="jobType">The runtime type of the recurring background job.</param>
        /// <param name="hostedService">The <see cref="IHostedService" /> instance to be executed as the background job.</param>
        public TypedServiceJob(Type jobType, IHostedService hostedService)
        {
            JobType = jobType;
            HostedService = hostedService;
        }

        /// <summary>
        /// Gets the runtime type of the recurring background job.
        /// </summary>
        /// <value>
        /// The job type.
        /// </value>
        public Type JobType { get; }

        /// <summary>
        /// Gets the hosted service instance associated with the job.
        /// </summary>
        /// <value>
        /// The hosted service.
        /// </value>
        public IHostedService HostedService { get; }
    }
}
