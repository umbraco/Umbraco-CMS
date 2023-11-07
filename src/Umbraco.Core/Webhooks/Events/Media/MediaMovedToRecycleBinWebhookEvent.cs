using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events.Media;

public class MediaMovedToRecycleBinWebhookEvent : WebhookEventContentBase<MediaMovedToRecycleBinNotification, IMedia>
{
    public MediaMovedToRecycleBinWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webHookService,
            webhookSettings,
            serverRoleAccessor,
            "Media Moved to Recycle Bin")
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaMovedToRecycleBinNotification notification) => notification.MoveInfoCollection.Select(x => x.Entity);

    protected override object ConvertEntityToRequestPayload(IMedia entity) => new DefaultPayloadModel { Id = entity.Key };
}
