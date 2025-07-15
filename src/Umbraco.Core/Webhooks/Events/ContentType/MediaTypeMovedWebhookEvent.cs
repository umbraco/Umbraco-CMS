using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Media Type Moved")]
public class MediaTypeMovedWebhookEvent : WebhookEventBase<MediaTypeMovedNotification>
{
    public MediaTypeMovedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MediaTypeMoved;

    public override object? ConvertNotificationToRequestPayload(MediaTypeMovedNotification notification)
        => notification.MoveInfoCollection.Select(moveEvent => new
        {
            Id = moveEvent.Entity.Key,
            NewParentId = moveEvent.NewParentKey,
        });
}
