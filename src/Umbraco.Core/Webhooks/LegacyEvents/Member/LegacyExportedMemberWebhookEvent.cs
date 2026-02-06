using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a member is exported, using the legacy payload format.
/// </summary>
[WebhookEvent("Member Exported")]
public class LegacyExportedMemberWebhookEvent : WebhookEventBase<ExportedMemberNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyExportedMemberWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyExportedMemberWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ExportedMember;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ExportedMemberNotification notification)
    {
        // No need to return the original member in the notification as well
        return new
        {
            exportedMember = notification.Exported
        };

    }
}
