using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.HostedServices;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="QueuedHostedService"/> class.
    /// </summary>
    /// <param name="taskQueue">The queue that provides background tasks to be processed.</param>
    /// <param name="logger">The logger used for logging information and errors related to the hosted service.</param>
    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedHostedService> logger)
    {
        TaskQueue = taskQueue;
        _logger = logger;
    }

    /// <summary>
    /// Gets the queue that manages background tasks for this hosted service.
    /// </summary>
    public IBackgroundTaskQueue TaskQueue { get; }

    /// <summary>
    /// Initiates a graceful shutdown of the hosted service.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> that is triggered when the host is performing a graceful shutdown.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous stop operation.</returns>
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

            if (workItem is null)
            {
                continue;
            }

            try
            {
                Task task;
                using (ExecutionContext.SuppressFlow())
                {
                    task = Task.Run(async () => await workItem(stoppingToken), stoppingToken);
                }

                await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred executing workItem.");
            }
        }
    }
}
