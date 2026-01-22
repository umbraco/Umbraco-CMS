using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Webhooks;
public sealed record WebhookContext(
    Uri Endpoint,
    string EventAlias,
    object Notification,
    IWebhook Webhook
);
