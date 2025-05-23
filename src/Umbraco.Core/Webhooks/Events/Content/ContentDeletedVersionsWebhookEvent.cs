using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Versions Deleted", Constants.WebhookEvents.Types.Content)]
public class ContentDeletedVersionsWebhookEvent : WebhookEventBase<ContentDeletedVersionsNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    public ContentDeletedVersionsWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IIdKeyMap idKeyMap)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _idKeyMap = idKeyMap;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedVersions;

    public override object? ConvertNotificationToRequestPayload(ContentDeletedVersionsNotification notification)
    {
        return new
        {
            Id = _idKeyMap.GetKeyForId(notification.Id, UmbracoObjectTypes.Document).Result,
            notification.DeletePriorVersions,
            notification.SpecificVersion,
            notification.DateToRetain,
        };
    }
}
