using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookBackgroundTaskQueue
{
    Task QueueBackgroundWorkItemAsync(Func<Task<WebhookResponseModel>> workItem);

    Task<Func<Task<WebhookResponseModel>>> DequeueAsync();
}
