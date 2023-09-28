using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaSaveWebhookEvent : WebhookEventBase<MediaSavedNotification, IMedia>
{
    public MediaSaveWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogRepository webhookLogRepository, string eventName)
        : base(webhookFiringService, webHookService, webhookLogRepository, eventName)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification) => notification.SavedEntities;
}
