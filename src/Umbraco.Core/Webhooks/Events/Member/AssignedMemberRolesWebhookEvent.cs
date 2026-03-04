using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when member roles are assigned.
/// </summary>
[WebhookEvent("Member Roles Assigned")]
public class AssignedMemberRolesWebhookEvent : WebhookEventBase<AssignedMemberRolesNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssignedMemberRolesWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="idKeyMap">The ID to key mapping service.</param>
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.AssignedMemberRoles;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(AssignedMemberRolesNotification notification)
        => new
        {
            Ids = notification.MemberIds.Select(id => _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Member).Result),
            notification.Roles,
        };
}
