using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Member Roles Removed")]
public class RemovedMemberRolesWebhookEvent : WebhookEventBase<RemovedMemberRolesNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    public RemovedMemberRolesWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IIdKeyMap idKeyMap)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
        _idKeyMap = idKeyMap;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.RemovedMemberRoles;

    public override object? ConvertNotificationToRequestPayload(RemovedMemberRolesNotification notification)
        => new
        {
            Ids = notification.MemberIds.Select(id => _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Member).Result),
            notification.Roles,
        };
}
