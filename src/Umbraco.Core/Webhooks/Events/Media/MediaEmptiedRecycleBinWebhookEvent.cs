using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events.Media;

public class MediaEmptiedRecycleBinWebhookEvent : WebhookEventContentBase<MediaEmptiedRecycleBinNotification, IMedia>
{
    public MediaEmptiedRecycleBinWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webHookService,
            webhookSettings,
            serverRoleAccessor,
            "Media Emptied Recycle Bin")
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaEmptiedRecycleBinNotification notification) => notification.DeletedEntities;

    protected override object ConvertEntityToRequestPayload(IMedia entity) => new DefaultPayloadModel { Id = entity.Key };
}
