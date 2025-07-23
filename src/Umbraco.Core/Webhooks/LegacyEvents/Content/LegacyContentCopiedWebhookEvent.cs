using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Copied", Constants.WebhookEvents.Types.Content)]
public class LegacyContentCopiedWebhookEvent : WebhookEventBase<ContentCopiedNotification>
{
    public LegacyContentCopiedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentCopied;

    public override object? ConvertNotificationToRequestPayload(ContentCopiedNotification notification)
    {
        return new
        {
            notification.Copy,
            notification.Original,
            notification.ParentId,
            notification.RelateToOriginal
        };
    }
}
