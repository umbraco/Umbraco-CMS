using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookFiringService
{
    Task<WebhookResponseModel> Fire(Webhook webhook, string eventName, object? requestObject);
}
