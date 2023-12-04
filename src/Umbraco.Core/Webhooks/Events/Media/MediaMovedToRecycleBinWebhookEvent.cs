using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Media Moved to Recycle Bin", Constants.WebhookEvents.Types.Media)]
public class MediaMovedToRecycleBinWebhookEvent : WebhookEventContentBase<MediaMovedToRecycleBinNotification, IMedia>
{
    public MediaMovedToRecycleBinWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webHookService,
            webhookSettings,
            serverRoleAccessor)
    {
    }

    public override string Alias => "mediaMovedToRecycleBin";

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaMovedToRecycleBinNotification notification) => notification.MoveInfoCollection.Select(x => x.Entity);

    protected override object ConvertEntityToRequestPayload(IMedia entity) => new DefaultPayloadModel { Id = entity.Key };
}
