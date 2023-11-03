using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookQueuedHostedService : RecurringHostedServiceBase
{
    private readonly IWebhookBackgroundTaskQueue _taskQueue;
    private readonly WebhookSettings _webhookSettings;

    public WebhookQueuedHostedService(ILogger<QueuedHostedService> logger, IWebhookBackgroundTaskQueue taskQueue, IOptions<WebhookSettings> webhookSettings)
        : base(logger, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1))
    {
        _taskQueue = taskQueue;
        _webhookSettings = webhookSettings.Value;
    }

    public override async Task PerformExecuteAsync(object? state) => await BackgroundProcessing();

    private async Task BackgroundProcessing()
    {

        while (await _taskQueue.DequeueAsync() is Func<Task<WebhookResponseModel>> workItem)
        {
            WebhookResponseModel response = await workItem();
            if (response.HttpResponseMessage.IsSuccessStatusCode || response.RetryCount >= _webhookSettings.MaximumRetries)
            {
                continue;
            }

            response.RetryCount++;
            await _taskQueue.QueueBackgroundWorkItemAsync(() => RetryWorkItem(workItem, response.RetryCount));
        }
    }

    private async Task<WebhookResponseModel> RetryWorkItem(Func<Task<WebhookResponseModel>> originalWorkItem, int retryCount)
    {
        WebhookResponseModel response = await originalWorkItem();
        response.RetryCount = retryCount;
        return response;
    }
}
