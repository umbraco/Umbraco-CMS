using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a stylesheet is deleted.
/// </summary>
[WebhookEvent("Stylesheet Deleted")]
public class StylesheetDeletedWebhookEvent : WebhookEventBase<StylesheetDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StylesheetDeletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public StylesheetDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.StylesheetDeleted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(StylesheetDeletedNotification notification)
        => notification.DeletedEntities.Select(entity => new DefaultPayloadModel { Id = entity.Key });
}
