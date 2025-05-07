using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Public Access Entry Saved")]
public class LegacyPublicAccessEntrySavedWebhookEvent : WebhookEventBase<PublicAccessEntrySavedNotification>
{
    public LegacyPublicAccessEntrySavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.PublicAccessEntrySaved;

    public override object? ConvertNotificationToRequestPayload(PublicAccessEntrySavedNotification notification)
        => notification.SavedEntities;
}
