namespace Umbraco.Cms.Core.Webhooks;

public interface IRetryService
{
    Task<HttpResponseMessage> RetryAsync(Func<Task<HttpResponseMessage>> action, int maxRetries = 3, TimeSpan? retryDelay = null);
}
