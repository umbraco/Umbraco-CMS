using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when an element is moved.
/// </summary>
[WebhookEvent("Element Moved", Constants.WebhookEvents.Types.Element)]
public class ElementMovedWebhookEvent : WebhookEventBase<ElementMovedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementMovedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ElementMovedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementMoved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ElementMovedNotification notification)
        => notification.MoveInfoCollection.Select(moveInfo => new DefaultPayloadModel { Id = moveInfo.Entity.Key });
}
