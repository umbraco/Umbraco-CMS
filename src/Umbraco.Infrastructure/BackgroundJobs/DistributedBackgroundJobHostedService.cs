using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs;

/// <summary>
///    A hosted service that checks for any runnable distributed background jobs on a timer.
/// </summary>
public class DistributedBackgroundJobHostedService : BackgroundService
{
    private readonly ILogger<DistributedBackgroundJobHostedService> _logger;
    private readonly IRuntimeState _runtimeState;

    public DistributedBackgroundJobHostedService(ILogger<DistributedBackgroundJobHostedService> logger, IRuntimeState runtimeState)
    {
        _logger = logger;
        _runtimeState = runtimeState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task DoWork()
    {
        if(_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }
    }
}
