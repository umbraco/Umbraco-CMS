using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Web.UI;

public class MyCustomWebhookPayloadProvider : WebhookPayloadProviderBase<ContentPublishedNotification>
{
    protected override bool CanHandle(Uri endpoint, string eventAlias, ContentPublishedNotification notification,
        IWebhook webhook)
    {
        var host = endpoint.Host.ToLowerInvariant();
        var isWebhookSite = host.Contains("webhook.site");
        return isWebhookSite;
    }

    protected override object BuildPayload(ContentPublishedNotification notification, Uri endpoint, string eventAlias,
        IWebhook webhook)
        => new {messgage = "Published new Content"};
}
