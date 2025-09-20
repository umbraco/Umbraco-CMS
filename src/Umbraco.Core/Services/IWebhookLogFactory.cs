using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookLogFactory
{
    Task<WebhookLog> CreateAsync(
        string eventAlias,
        HttpRequestMessage requestMessage,
        HttpResponseMessage? httpResponseMessage,
        int retryCount,
        Exception? exception,
        IWebhook webhook,
        CancellationToken cancellationToken);
}
