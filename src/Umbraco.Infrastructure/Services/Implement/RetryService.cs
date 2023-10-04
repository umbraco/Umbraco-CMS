using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public class RetryService : IRetryService
{
    public async Task<WebhookResponseModel> RetryAsync(Func<Task<HttpResponseMessage>> action, int maxRetries = 5, TimeSpan? retryDelay = null)
    {
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                HttpResponseMessage response = await action();
                return new WebhookResponseModel
                {
                    RetryCount = retry,
                    HttpResponseMessage = response,
                };
            }
            catch (Exception ex)
            {
                // Retry after a delay, if specified
                if (retryDelay != null)
                {
                    await Task.Delay(retryDelay.Value);
                }
            }
        }

        return new WebhookResponseModel
        {
            RetryCount = maxRetries,
            HttpResponseMessage = null!,
        };
        // TODO: Every retry failed, should we log some errors here, maybe the error in the catch?
    }
}
