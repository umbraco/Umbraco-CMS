using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a media type is moved.
/// </summary>
[WebhookEvent("Media Type Moved")]
public class MediaTypeMovedWebhookEvent : WebhookEventBase<MediaTypeMovedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeMovedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public MediaTypeMovedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.MediaTypeMoved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(MediaTypeMovedNotification notification)
        => notification.MoveInfoCollection.Select(moveEvent => new
        {
            Id = moveEvent.Entity.Key,
            NewParentId = moveEvent.NewParentKey,
        });
}
