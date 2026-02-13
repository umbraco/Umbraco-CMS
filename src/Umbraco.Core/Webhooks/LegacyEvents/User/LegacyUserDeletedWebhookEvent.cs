using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a user is deleted, using the legacy payload format.
/// </summary>
[WebhookEvent("User Deleted")]
public class LegacyUserDeletedWebhookEvent : WebhookEventBase<UserDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyUserDeletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyUserDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.UserDeleted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(UserDeletedNotification notification)
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
