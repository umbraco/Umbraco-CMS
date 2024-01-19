using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Member Exported")]
public class ExportedMemberWebhookEvent : WebhookEventBase<ExportedMemberNotification>
{
    public ExportedMemberWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => "exportedMember";

    public override object? ConvertNotificationToRequestPayload(ExportedMemberNotification notification)
    {
        // No need to return the original member in the notification as well
        return new
        {
            exportedMember = notification.Exported
        };

    }
}
