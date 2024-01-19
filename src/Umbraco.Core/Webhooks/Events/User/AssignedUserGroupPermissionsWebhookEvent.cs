using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("User Group Permissions Assigned")]
public class AssignedUserGroupPermissionsWebhookEvent : WebhookEventBase<AssignedUserGroupPermissionsNotification>
{
    public AssignedUserGroupPermissionsWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => "assignedUserGroupPermissions";

    public override object? ConvertNotificationToRequestPayload(AssignedUserGroupPermissionsNotification notification)
        => notification.EntityPermissions;
}
