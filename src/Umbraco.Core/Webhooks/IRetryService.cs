using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Webhooks;

public interface IRetryService
{
    Task<WebhookResponseModel> RetryAsync(Func<Task<HttpResponseMessage>> action, int maxRetries = 3, TimeSpan? retryDelay = null);
}
