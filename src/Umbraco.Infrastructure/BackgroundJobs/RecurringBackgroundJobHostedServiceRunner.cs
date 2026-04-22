using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
///     A hosted service that discovers and starts hosted services for any recurring background jobs in the DI container.
/// </summary>
public class RecurringBackgroundJobHostedServiceRunner : IHostedService
{
    private readonly ILogger<RecurringBackgroundJobHostedServiceRunner> _logger;
    private readonly List<IRecurringBackgroundJob> _jobs;
    private readonly Func<IRecurringBackgroundJob, IHostedService> _jobFactory;
    private readonly List<NamedServiceJob> _hostedServices = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="RecurringBackgroundJobHostedServiceRunner"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger{RecurringBackgroundJobHostedServiceRunner}"/> used for logging within the runner.</param>
    /// <param name="jobs">A collection of <see cref="IRecurringBackgroundJob"/> instances to be managed by the runner.</param>
    /// <param name="jobFactory">A factory function that creates an <see cref="IHostedService"/> for each <see cref="IRecurringBackgroundJob"/>.</param>
    public RecurringBackgroundJobHostedServiceRunner(
        ILogger<RecurringBackgroundJobHostedServiceRunner> logger,
        IEnumerable<IRecurringBackgroundJob> jobs,
        Func<IRecurringBackgroundJob, IHostedService> jobFactory)
    {
        _jobs = jobs.ToList();
        _logger = logger;
        _jobFactory = jobFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting recurring background jobs hosted services");

        foreach (IRecurringBackgroundJob job in _jobs)
        {
            var jobName = job.GetType().Name;
            try
            {

                _logger.LogDebug("Creating background hosted service for {job}", jobName);
                IHostedService hostedService = _jobFactory(job);

                _logger.LogInformation("Starting a background hosted service for {job} with a delay of {delay}, running every {period}", jobName, job.Delay, job.Period);

                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);

                _hostedServices.Add(new NamedServiceJob(jobName, hostedService));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to start background hosted service for {job}", jobName);
            }
        }

        _logger.LogInformation("Completed starting recurring background jobs hosted services");
    }

    /// <summary>
    /// Asynchronously stops all recurring background job hosted services managed by this runner.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> that can be used to cancel the stop operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous stop operation.</returns>
    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping recurring background jobs hosted services");

        foreach (NamedServiceJob namedServiceJob in _hostedServices)
        {
            try
            {
                _logger.LogInformation("Stopping background hosted service for {job}", namedServiceJob.Name);
                await namedServiceJob.HostedService.StopAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to stop background hosted service for {job}", namedServiceJob.Name);
            }
        }

        _logger.LogInformation("Completed stopping recurring background jobs hosted services");
    }

    private sealed class NamedServiceJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedServiceJob"/> class using the specified job name and hosted service instance.
        /// </summary>
        /// <param name="name">The unique name identifying the job.</param>
        /// <param name="hostedService">The <see cref="IHostedService"/> instance to be executed as the background job.</param>
        public NamedServiceJob(string name, IHostedService hostedService)
        {
            Name = name;
            HostedService = hostedService;
        }

        /// <summary>
        /// Gets the unique name that identifies this background job.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hosted service instance associated with the named service job.
        /// </summary>
        public IHostedService HostedService { get; }
    }
}
