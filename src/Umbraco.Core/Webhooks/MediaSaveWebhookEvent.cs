using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaSaveWebhookEvent : WebhookEventBase<MediaSavedNotification, IMedia>
{
    public MediaSaveWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService, IOptionsMonitor<WebhookSettings> webhookSettings)
        : base(webhookFiringService, webHookService, webhookLogService, webhookSettings, Constants.WebhookEvents.MediaSave)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification) => notification.SavedEntities;
}
