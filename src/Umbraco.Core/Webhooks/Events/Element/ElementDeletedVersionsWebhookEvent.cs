using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when element versions are deleted.
/// </summary>
[WebhookEvent("Element Versions Deleted", Constants.WebhookEvents.Types.Element)]
public class ElementDeletedVersionsWebhookEvent : WebhookEventBase<ElementDeletedVersionsNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementDeletedVersionsWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ElementDeletedVersionsWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementDeletedVersions;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ElementDeletedVersionsNotification notification) =>
        new
        {
            Id = notification.Key,
            notification.DeletePriorVersions,
            notification.SpecificVersion,
            notification.DateToRetain,
        };
}
