using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a document type is changed.
/// </summary>
[WebhookEvent("Document Type Changed")]
public class DocumentTypeChangedWebhookEvent : WebhookEventBase<ContentTypeChangedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeChangedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public DocumentTypeChangedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.DocumentTypeChanged;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentTypeChangedNotification notification)
        => notification.Changes.Select(contentTypeChange => new
        {
            Id = contentTypeChange.Item.Key,
            ContentTypeChange = contentTypeChange.ChangeTypes,
        });
}
