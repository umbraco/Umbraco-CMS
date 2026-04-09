using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a partial view is saved, using the legacy payload format.
/// </summary>
[WebhookEvent("Partial View Saved")]
public class LegacyPartialViewSavedWebhookEvent : WebhookEventBase<PartialViewSavedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyPartialViewSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyPartialViewSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.PartialViewSaved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(PartialViewSavedNotification notification) =>
        notification.SavedEntities;
}
