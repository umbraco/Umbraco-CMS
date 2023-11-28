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
    private readonly List<(IHostedService HostedService, string JobName)> _hostedServices = new();


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

                _logger.LogInformation("Creating background hosted service for {job}", jobName);
                IHostedService hostedService = _jobFactory(job);

                _logger.LogInformation("Starting background hosted service for {job}", jobName);
                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);

                _hostedServices.Add((hostedService, jobName));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to start background hosted service for {job}", jobName);
            }
        }

        _logger.LogInformation("Completed starting recurring background jobs hosted services");
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping recurring background jobs hosted services");

        foreach ((IHostedService HostedService, string JobName) pair in _hostedServices)
        {
            try
            {
                _logger.LogInformation("Stopping background hosted service for {job}", pair.JobName);
                await pair.HostedService.StopAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to stop background hosted service for {job}", pair.JobName);
            }
        }

        _logger.LogInformation("Completed stopping recurring background jobs hosted services");

    }
}
