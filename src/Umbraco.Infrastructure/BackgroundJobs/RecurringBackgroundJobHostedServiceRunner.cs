using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
///     A hosted service that discovers and starts hosted services for any recurring background jobs in the DI container.
/// </summary>
public class RecurringBackgroundJobHostedServiceRunner : IHostedService
{
    private readonly ILogger<RecurringBackgroundJobHostedServiceRunner> _logger;
    private readonly List<IRecurringBackgroundJob> _jobs;
    private readonly Func<IRecurringBackgroundJob, IHostedService> _jobFactory;
    private IList<IHostedService> _hostedServices = new List<IHostedService>();


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
        _logger.LogInformation("Creating recurring background jobs hosted services");

        // create hosted services for each background job
        _hostedServices = _jobs.Select(_jobFactory).ToList();

        _logger.LogInformation("Starting recurring background jobs hosted services");

        foreach (IHostedService hostedService in _hostedServices)
        {
            try
            {
                _logger.LogInformation($"Starting background hosted service for {hostedService.GetType().Name}");
                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to start background hosted service for {hostedService.GetType().Name}");
            }
        }

        _logger.LogInformation("Completed starting recurring background jobs hosted services");


    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping recurring background jobs hosted services");

        foreach (IHostedService hostedService in _hostedServices)
        {
            try
            {
                _logger.LogInformation($"Stopping background hosted service for {hostedService.GetType().Name}");
                await hostedService.StopAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to stop background hosted service for {hostedService.GetType().Name}");
            }
        }

        _logger.LogInformation("Completed stopping recurring background jobs hosted services");

    }
}
