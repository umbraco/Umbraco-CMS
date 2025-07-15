using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Public Access Entry Deleted")]
public class LegacyPublicAccessEntryDeletedWebhookEvent : WebhookEventBase<PublicAccessEntryDeletedNotification>
{
    public LegacyPublicAccessEntryDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.PublicAccessEntryDeleted;

    public override object? ConvertNotificationToRequestPayload(PublicAccessEntryDeletedNotification notification) => notification.DeletedEntities;
}
