using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when user group permissions are assigned.
/// </summary>
[WebhookEvent("User Group Permissions Assigned")]
public class AssignedUserGroupPermissionsWebhookEvent : WebhookEventBase<AssignedUserGroupPermissionsNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssignedUserGroupPermissionsWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="idKeyMap">The ID to key mapping service.</param>
    public AssignedUserGroupPermissionsWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IIdKeyMap idKeyMap)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.AssignedUserGroupPermissions;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(AssignedUserGroupPermissionsNotification notification)
        => notification.EntityPermissions.Select(permission =>
        new
        {
            UserId = _idKeyMap.GetKeyForId(permission.EntityId, UmbracoObjectTypes.Unknown).Result,
            UserGroupId = _idKeyMap.GetKeyForId(permission.UserGroupId, UmbracoObjectTypes.Unknown).Result,
        });
}
