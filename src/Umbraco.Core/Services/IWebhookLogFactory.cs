using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookLogFactory
{
    Task<WebhookLog> CreateAsync(string eventName, WebhookResponseModel responseModel, Webhook webhook);
}
