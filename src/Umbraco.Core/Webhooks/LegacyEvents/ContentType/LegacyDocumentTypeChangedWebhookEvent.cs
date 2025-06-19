using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Document Type Changed")]
public class LegacyDocumentTypeChangedWebhookEvent : WebhookEventBase<ContentTypeChangedNotification>
{
    public LegacyDocumentTypeChangedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.DocumentTypeChanged;

    public override object? ConvertNotificationToRequestPayload(ContentTypeChangedNotification notification)
        => notification.Changes;
}
