using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookFiringService
{
    Task<WebhookResponseModel> Fire( string url, string eventName, object? requestObject);
}
