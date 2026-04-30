using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when an element is copied.
/// </summary>
[WebhookEvent("Element Copied", Constants.WebhookEvents.Types.Element)]
public class ElementCopiedWebhookEvent : WebhookEventBase<ElementCopiedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCopiedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ElementCopiedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementCopied;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ElementCopiedNotification notification)
    {
        return new
        {
            Id = notification.Copy.Key,
            Original = notification.Original.Key,
            Parent = notification.ParentKey,
            notification.RelateToOriginal,
        };
    }
}
