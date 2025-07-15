using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Group Permissions Assigned")]
public class LegacyAssignedUserGroupPermissionsWebhookEvent : WebhookEventBase<AssignedUserGroupPermissionsNotification>
{
    public LegacyAssignedUserGroupPermissionsWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.AssignedUserGroupPermissions;

    public override object? ConvertNotificationToRequestPayload(AssignedUserGroupPermissionsNotification notification)
        => notification.EntityPermissions;
}
