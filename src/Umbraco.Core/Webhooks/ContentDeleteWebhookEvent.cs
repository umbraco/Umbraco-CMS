using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentDeleteWebhookEvent : WebhookEventBase<ContentDeletedNotification, IContent>
{
    public ContentDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService)
        : base(webhookFiringService, webHookService, webhookLogService, Constants.WebhookEvents.ContentDelete)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedNotification notification) => notification.DeletedEntities;
}
