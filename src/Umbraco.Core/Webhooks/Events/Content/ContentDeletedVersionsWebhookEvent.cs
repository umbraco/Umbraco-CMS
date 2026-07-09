using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when content versions are deleted.
/// </summary>
[WebhookEvent("Content Versions Deleted", Constants.WebhookEvents.Types.Content)]
public class ContentDeletedVersionsWebhookEvent : WebhookEventBase<ContentDeletedVersionsNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDeletedVersionsWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="idKeyMap">The ID to key mapping service.</param>
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedVersions;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentDeletedVersionsNotification notification)
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
