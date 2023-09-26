using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public class RetryService : IRetryService
{
    public async Task<HttpResponseMessage> RetryAsync(Func<Task<HttpResponseMessage>> action, int maxRetries = 3, TimeSpan? retryDelay = null)
    {
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                return await action();
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

        return null!;
        // TODO: Every retry failed, should we log some errors here, maybe the error in the catch?
    }
}
