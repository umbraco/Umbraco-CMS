using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookRequestService
{
    Task<WebhookRequest> CreateAsync(Guid webhookKey, string eventAlias, object? payload);

    Task<IEnumerable<WebhookRequest>> GetAllAsync();

    Task DeleteAsync(WebhookRequest webhookRequest);

    Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest);
}
