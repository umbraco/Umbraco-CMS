using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Relation Type Deleted")]
public class RelationTypeDeletedWebhookEvent : WebhookEventBase<RelationTypeDeletedNotification>
{
    public RelationTypeDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }


    public override string Alias => "relationTypeDeleted";

    public override object? ConvertNotificationToRequestPayload(RelationTypeDeletedNotification notification)
        => notification.DeletedEntities;
}
