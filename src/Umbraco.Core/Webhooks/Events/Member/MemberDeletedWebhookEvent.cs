using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Member Deleted")]
public class MemberDeletedWebhookEvent : WebhookEventBase<MemberDeletedNotification>
{
    public MemberDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => "memberDeleted";

    public override object? ConvertNotificationToRequestPayload(MemberDeletedNotification notification)
    {
        // TODO: Map more stuff here
        var result = notification.DeletedEntities.Select(entity => new
        {
            entity.Id,
            entity.Key,
            entity.Name,
            entity.ContentTypeAlias,
            entity.Email,
            entity.Username,
            entity.FailedPasswordAttempts
        });

        return result;
    }
}
