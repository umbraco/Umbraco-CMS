using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Forgot Password Requested")]
public class UserForgotPasswordRequestedWebhookEvent : WebhookEventBase<UserForgotPasswordRequestedNotification>
{
    private readonly IUserService _userService;

    public UserForgotPasswordRequestedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IUserService userService)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
        _userService = userService;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.UserForgotPasswordRequested;

    public override object? ConvertNotificationToRequestPayload(UserForgotPasswordRequestedNotification notification)
        => new DefaultPayloadModel
        {
            Id = notification.AffectedUserId is not null &&
                 Guid.TryParse(notification.AffectedUserId, out Guid affectedUserGuid)
                ? affectedUserGuid
                : Guid.Empty,
        };
}
