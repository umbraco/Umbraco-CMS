using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookFiringService
{
    Task FireAsync(IWebhook webhook, string eventAlias, object? payload, CancellationToken cancellationToken);
}
