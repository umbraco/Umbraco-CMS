using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Group Permissions Assigned")]
public class AssignedUserGroupPermissionsWebhookEvent : WebhookEventBase<AssignedUserGroupPermissionsNotification>
{
    private readonly IIdKeyMap _idKeyMap;

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

    public override string Alias => Constants.WebhookEvents.Aliases.AssignedUserGroupPermissions;

    public override object? ConvertNotificationToRequestPayload(AssignedUserGroupPermissionsNotification notification)
        => notification.EntityPermissions.Select(permission =>
        new {
            UserId = _idKeyMap.GetKeyForId(permission.EntityId, UmbracoObjectTypes.Unknown).Result,
            UserGroupId = _idKeyMap.GetKeyForId(permission.UserGroupId, UmbracoObjectTypes.Unknown).Result,
        });
}
