using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a user successfully logs out.
/// </summary>
[WebhookEvent("User Logout Success")]
public class UserLogoutSuccessWebhookEvent : WebhookEventBase<UserLogoutSuccessNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserLogoutSuccessWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public UserLogoutSuccessWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.UserLogoutSuccess;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(UserLogoutSuccessNotification notification)
        => new DefaultPayloadModel
        {
            Id = notification.AffectedUserId is not null &&
                 Guid.TryParse(notification.AffectedUserId, out Guid affectedUserGuid)
                ? affectedUserGuid
                : Guid.Empty,
        };
}
