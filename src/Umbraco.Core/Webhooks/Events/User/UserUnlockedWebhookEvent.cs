using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a user is unlocked.
/// </summary>
[WebhookEvent("User Unlocked")]
public class UserUnlockedWebhookEvent : WebhookEventBase<UserUnlockedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserUnlockedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public UserUnlockedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.UserUnlocked;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(UserUnlockedNotification notification)
        => new
        {
            Id = notification.AffectedUserId is not null &&
                 Guid.TryParse(notification.AffectedUserId, out Guid affectedUserGuid)
                ? affectedUserGuid
                : Guid.Empty,
            PerformingId = Guid.TryParse(notification.AffectedUserId, out Guid performingUserGuid)
                ? performingUserGuid
                : Guid.Empty,
        };
}
