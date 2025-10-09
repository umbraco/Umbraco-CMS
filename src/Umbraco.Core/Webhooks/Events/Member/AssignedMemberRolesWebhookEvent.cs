using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Member Roles Assigned")]
public class AssignedMemberRolesWebhookEvent : WebhookEventBase<AssignedMemberRolesNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    public AssignedMemberRolesWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IIdKeyMap idKeyMap)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
        _idKeyMap = idKeyMap;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.AssignedMemberRoles;

    public override object? ConvertNotificationToRequestPayload(AssignedMemberRolesNotification notification)
        => new
        {
            Ids = notification.MemberIds.Select(id => _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Member).Result),
            notification.Roles,
        };
}
