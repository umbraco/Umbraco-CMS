using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Saved")]
public class UserSavedWebhookEvent : WebhookEventBase<UserSavedNotification>
{
    public UserSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.UserSaved;

    public override object? ConvertNotificationToRequestPayload(UserSavedNotification notification)
    {
        // TODO: Map more stuff here
        var result = notification.SavedEntities.Select(entity => new
        {
            entity.Id,
            entity.Key,
            entity.Name,
            entity.Language,
            entity.Email,
            entity.Username,
            entity.FailedPasswordAttempts
        });

        return result;
    }
}
