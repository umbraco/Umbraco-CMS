using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HostedServices;

public class WebhookQueueExecuteHostedService : BackgroundService
{
    private readonly IWebhookBackgroundTaskQueue _taskQueue;
    private readonly WebhookSettings _webhookSettings;

    public WebhookQueueExecuteHostedService(ILogger<QueuedHostedService> logger, IWebhookBackgroundTaskQueue taskQueue, IOptions<WebhookSettings> webhookSettings)
    {
        _taskQueue = taskQueue;
        _webhookSettings = webhookSettings.Value;
    }

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
            await Task.Delay(CalculateDelay(response.RetryCount));
        }
    }

    private async Task<WebhookResponseModel> RetryWorkItem(Func<Task<WebhookResponseModel>> originalWorkItem, int retryCount)
    {
        WebhookResponseModel response = await originalWorkItem();
        response.RetryCount = retryCount;
        return response;
    }

    private TimeSpan CalculateDelay(int retryCount)
    {
        if (retryCount < _webhookSettings.RetryDelaysInMilliseconds.Length)
        {
            var delay = _webhookSettings.RetryDelaysInMilliseconds[retryCount];
            return TimeSpan.FromMilliseconds(delay);
        }
        else
        {
            var delay = _webhookSettings.RetryDelaysInMilliseconds.Last();
            return TimeSpan.FromMilliseconds(delay);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await BackgroundProcessing();
}
