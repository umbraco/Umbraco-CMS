using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentPublishWebhookEvent : WebhookEventBase<ContentPublishedNotification, IContent>
{
    public ContentPublishWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService, IOptionsMonitor<WebhookSettings> webhookSettings)
        : base(webhookFiringService, webHookService, webhookLogService, webhookSettings, Constants.WebhookEvents.ContentPublish)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) => notification.PublishedEntities;
}
