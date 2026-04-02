using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a language is deleted, using the legacy payload format.
/// </summary>
[WebhookEvent("Language Deleted")]
public class LegacyLanguageDeletedWebhookEvent : WebhookEventBase<LanguageDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyLanguageDeletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyLanguageDeletedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.LanguageDeleted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(LanguageDeletedNotification notification)
        => notification.DeletedEntities;
}
