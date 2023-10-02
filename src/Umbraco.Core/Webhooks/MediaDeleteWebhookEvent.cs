using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaDeleteWebhookEvent : WebhookEventBase<MediaDeletedNotification, IMedia>
{
    public MediaDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService)
        : base(webhookFiringService, webHookService, webhookLogService, Constants.WebhookEvents.MediaDelete)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaDeletedNotification notification) => notification.DeletedEntities;
}
