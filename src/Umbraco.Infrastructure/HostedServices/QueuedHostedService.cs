using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     A queue based hosted service used to executing tasks on a background thread.
/// </summary>
/// <remarks>
///     Borrowed from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0
/// </remarks>
public class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        TaskQueue = taskQueue;
        _logger = logger;
    }

    public IBackgroundTaskQueue TaskQueue { get; }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await BackgroundProcessing(stoppingToken);

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Func<CancellationToken, Task>? workItem = await TaskQueue.DequeueAsync(stoppingToken);

            try
            {
                if (workItem is not null)
                {
                    await workItem(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred executing {WorkItem}.", nameof(workItem));
            }
        }
    }
}
