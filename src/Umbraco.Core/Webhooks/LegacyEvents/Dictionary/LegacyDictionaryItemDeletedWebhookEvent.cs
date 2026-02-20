using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a dictionary item is deleted, using the legacy payload format.
/// </summary>
[WebhookEvent("Dictionary Item Deleted")]
public class LegacyDictionaryItemDeletedWebhookEvent : WebhookEventBase<DictionaryItemDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyDictionaryItemDeletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyDictionaryItemDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.DictionaryItemDeleted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(DictionaryItemDeletedNotification notification)
        => notification.DeletedEntities;
}
