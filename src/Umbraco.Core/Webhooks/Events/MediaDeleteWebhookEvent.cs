using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

public class MediaDeleteWebhookEvent : WebhookEventBase<MediaDeletedNotification, IMedia>
{
    public MediaDeleteWebhookEvent(
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
            Constants.WebhookEvents.MediaDelete)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaDeletedNotification notification) => notification.DeletedEntities;

    protected override object ConvertEntityToRequestPayload(IMedia entity) => new DefaultPayloadModel { Id = entity.Key };
}
