using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a document type is moved.
/// </summary>
[WebhookEvent("Document Type Moved")]
public class DocumentTypeMovedWebhookEvent : WebhookEventBase<ContentTypeMovedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeMovedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public DocumentTypeMovedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.DocumentTypeMoved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentTypeMovedNotification notification)
        => notification.MoveInfoCollection.Select(moveEvent => new
        {
            Id = moveEvent.Entity.Key,
            NewParentId = moveEvent.NewParentKey,
        });
}
