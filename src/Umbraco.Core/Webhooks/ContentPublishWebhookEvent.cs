using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentPublishWebhookEvent : WebhookEventBase<ContentPublishedNotification, IContent>
{
    public ContentPublishWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService)
        : base(webhookFiringService, webHookService, webhookLogService, Constants.WebhookEvents.ContentPublish)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) => notification.PublishedEntities;
}
