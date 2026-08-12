using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when element versions are deleted.
/// </summary>
[WebhookEvent("Element Versions Deleted", Constants.WebhookEvents.Types.Element)]
public class ElementDeletedVersionsWebhookEvent : WebhookEventBase<ElementDeletedVersionsNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementDeletedVersionsWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="idKeyMap">The ID to key mapping service.</param>
    public ElementDeletedVersionsWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementDeletedVersions;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ElementDeletedVersionsNotification notification)
    {
        return new
        {
            Id = _idKeyMap.GetKeyForId(notification.Id, UmbracoObjectTypes.Element).Result,
            notification.DeletePriorVersions,
            notification.SpecificVersion,
            notification.DateToRetain,
        };
    }
}
