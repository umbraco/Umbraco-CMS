using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when content versions are deleted, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Versions Deleted", Constants.WebhookEvents.Types.Content)]
public class LegacyContentDeletedVersionsWebhookEvent : WebhookEventBase<ContentDeletedVersionsNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentDeletedVersionsWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyContentDeletedVersionsWebhookEvent(
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedVersions;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentDeletedVersionsNotification notification)
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
