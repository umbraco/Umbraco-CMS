using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when content is moved to the recycle bin.
/// </summary>
[WebhookEvent("Content Moved to Recycle Bin", Constants.WebhookEvents.Types.Content)]
public class ContentMovedToRecycleBinWebhookEvent : WebhookEventBase<ContentMovedToRecycleBinNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentMovedToRecycleBinWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ContentMovedToRecycleBinWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentMovedToRecycleBin;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentMovedToRecycleBinNotification notification)
        => notification.MoveInfoCollection.Select(moveInfo => new DefaultPayloadModel { Id = moveInfo.Entity.Key });
}
