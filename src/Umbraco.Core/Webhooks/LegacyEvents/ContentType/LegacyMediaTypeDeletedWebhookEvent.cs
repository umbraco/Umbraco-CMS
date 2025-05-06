using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Media Type Deleted")]
public class LegacyMediaTypeDeletedWebhookEvent : WebhookEventBase<MediaTypeDeletedNotification>
{
    public LegacyMediaTypeDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MediaTypeDeleted;

    public override object? ConvertNotificationToRequestPayload(MediaTypeDeletedNotification notification)
        => notification.DeletedEntities;
}
