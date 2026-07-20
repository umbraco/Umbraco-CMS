using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a user group is saved.
/// </summary>
[WebhookEvent("User Group Saved")]
public class UserGroupSavedWebhookEvent : WebhookEventBase<UserGroupSavedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserGroupSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public UserGroupSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.UserGroupSaved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(UserGroupSavedNotification notification)
        => notification.SavedEntities.Select(entity => new DefaultPayloadModel { Id = entity.Key });
}
