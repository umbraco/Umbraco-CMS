using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a member group is saved, using the legacy payload format.
/// </summary>
[WebhookEvent("Member Group Saved")]
public class LegacyMemberGroupSavedWebhookEvent : WebhookEventBase<MemberGroupSavedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyMemberGroupSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyMemberGroupSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.MemberGroupSaved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(MemberGroupSavedNotification notification)
        => notification.SavedEntities;
}
