using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Versions Deleted", Constants.WebhookEvents.Types.Content)]
public class ContentDeletedVersionsWebhookEvent : WebhookEventBase<ContentDeletedVersionsNotification>
{
    public ContentDeletedVersionsWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedVersions;

    public override object? ConvertNotificationToRequestPayload(ContentDeletedVersionsNotification notification)
    {
        return new
        {
            notification.Id,
            notification.DeletePriorVersions,
            notification.SpecificVersion,
            notification.DateToRetain
        };
    }
}
