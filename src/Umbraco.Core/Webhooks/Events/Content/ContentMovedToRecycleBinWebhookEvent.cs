using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Moved to Recycle Bin", Constants.WebhookEvents.Types.Content)]
public class ContentMovedToRecycleBinWebhookEvent : WebhookEventBase<ContentMovedToRecycleBinNotification>
{
    public ContentMovedToRecycleBinWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentMovedToRecycleBin;

    public override object? ConvertNotificationToRequestPayload(ContentMovedToRecycleBinNotification notification)
        => notification.MoveInfoCollection;
}
