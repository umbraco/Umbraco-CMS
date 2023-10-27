using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

public class MediaSaveWebhookEvent : WebhookEventBase<MediaSavedNotification, IMedia>
{
    public MediaSaveWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IWebhookLogService webhookLogService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookLogFactory webhookLogFactory,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webHookService,
            webhookLogService,
            webhookSettings,
            webhookLogFactory,
            serverRoleAccessor,
            Constants.WebhookEvents.MediaSave)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification) => notification.SavedEntities;
}
