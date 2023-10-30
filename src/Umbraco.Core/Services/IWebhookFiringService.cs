using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookFiringService
{
    Task FireAsync(Webhook webhook, string eventName, object? payload, CancellationToken cancellationToken, TimeSpan? retryDelay = null);
}
