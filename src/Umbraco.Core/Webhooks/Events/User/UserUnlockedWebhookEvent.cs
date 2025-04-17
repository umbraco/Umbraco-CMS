using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Unlocked")]
public class UserUnlockedWebhookEvent : WebhookEventBase<UserUnlockedNotification>
{
    public UserUnlockedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.UserUnlocked;

    public override object? ConvertNotificationToRequestPayload(UserUnlockedNotification notification)
        => new
        {
            Id = notification.AffectedUserId is not null &&
                 Guid.TryParse(notification.AffectedUserId, out Guid affectedUserGuid)
                ? affectedUserGuid
                : Guid.Empty,
            PerormingId = Guid.TryParse(notification.AffectedUserId, out Guid performingUserGuid)
                ? performingUserGuid
                : Guid.Empty,
        };
}
