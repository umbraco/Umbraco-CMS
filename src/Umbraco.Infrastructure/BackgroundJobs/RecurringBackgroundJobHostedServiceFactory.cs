using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
///     A queue based hosted service used to executing tasks on a background thread.
/// </summary>
/// <remarks>
///     Borrowed from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0
/// </remarks>
public class RecurringBackgroundJobHostedServiceFactory : IHostedService
{
    private readonly ILogger<RecurringBackgroundJobHostedServiceFactory> _logger;
    private readonly IList<IRecurringBackgroundJob> _jobs;
    private IList<RecurringBackgroundJobHostedService> _hostedServices = new List<RecurringBackgroundJobHostedService>();
    private readonly ILoggerFactory _loggerFactory;

    private readonly IMainDom _mainDom;
    private readonly IRuntimeState _runtimeState;
    private readonly IServerRoleAccessor _serverRoleAccessor;

    public RecurringBackgroundJobHostedServiceFactory(
        IRuntimeState runtimeState,
        ILogger<RecurringBackgroundJobHostedServiceFactory> logger,
        IMainDom mainDom,
        IServerRoleAccessor serverRoleAccessor,
        ILoggerFactory loggerFactory,
        IEnumerable<IRecurringBackgroundJob> jobs)
    {
        _jobs = jobs.ToList();
        _logger = logger;

        _loggerFactory = loggerFactory;

        _mainDom = mainDom;
        _runtimeState = runtimeState;
        _serverRoleAccessor = serverRoleAccessor;

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating recurring background jobs hosted services");

        // create hosted services for each background job
        _hostedServices = _jobs.Select(job =>
        {
            return new RecurringBackgroundJobHostedService(
            _runtimeState,
                _loggerFactory.CreateLogger<RecurringBackgroundJobHostedService>(),
                _mainDom,
                _serverRoleAccessor,
                job);
        }).ToList();

        _logger.LogInformation("Starting recurring background jobs hosted services");

        foreach (RecurringBackgroundJobHostedService hostedService in _hostedServices)
        {
            try
            {
                _logger.LogInformation($"Starting background hosted service for {hostedService.JobName}");
                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to start background hosted service for {hostedService.JobName}");
            }
        }

        _logger.LogInformation("Completed starting recurring background jobs hosted services");


    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping recurring background jobs hosted services");

        foreach (RecurringBackgroundJobHostedService hostedService in _hostedServices)
        {
            try
            {
                _logger.LogInformation($"Stopping background hosted service for {hostedService.JobName}");
                await hostedService.StopAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to stop background hosted service for {hostedService.JobName}");
            }
        }

        _logger.LogInformation("Completed stopping recurring background jobs hosted services");

    }



}
