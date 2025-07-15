using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Member Group Deleted")]
public class LegacyMemberGroupDeletedWebhookEvent : WebhookEventBase<MemberGroupDeletedNotification>
{
    public LegacyMemberGroupDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MemberGroupDeleted;

    public override object? ConvertNotificationToRequestPayload(MemberGroupDeletedNotification notification)
        => notification.DeletedEntities;
}
