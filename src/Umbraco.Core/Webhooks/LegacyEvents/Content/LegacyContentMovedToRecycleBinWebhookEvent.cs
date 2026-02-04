using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when content is moved to the recycle bin, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Moved to Recycle Bin", Constants.WebhookEvents.Types.Content)]
public class LegacyContentMovedToRecycleBinWebhookEvent : WebhookEventBase<ContentMovedToRecycleBinNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentMovedToRecycleBinWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyContentMovedToRecycleBinWebhookEvent(
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
        => notification.MoveInfoCollection;
}
