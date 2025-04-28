using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Deleted")]
public class LegacyUserDeletedWebhookEvent : WebhookEventBase<UserDeletedNotification>
{
    public LegacyUserDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.UserDeleted;

    public override object? ConvertNotificationToRequestPayload(UserDeletedNotification notification)
    {
        var result = notification.DeletedEntities.Select(entity => new
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
