using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookQueuedHostedService : RecurringHostedServiceBase
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IWebhookBackgroundTaskQueue _taskQueue;

    public WebhookQueuedHostedService(ILogger<QueuedHostedService> logger, IWebhookBackgroundTaskQueue taskQueue) : base(logger, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1))
    {
        _logger = logger;
        _taskQueue = taskQueue;
    }

    public override async Task PerformExecuteAsync(object? state) => await BackgroundProcessing();

    private async Task BackgroundProcessing()
    {
        Func<Task<WebhookResponseModel>> workItem = await _taskQueue.DequeueAsync();

        try
        {
            WebhookResponseModel response = await workItem();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing {WorkItem}.", nameof(workItem));
        }
    }
}
