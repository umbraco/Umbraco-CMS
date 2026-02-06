using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a member type is saved.
/// </summary>
[WebhookEvent("Member Type Saved")]
public class MemberTypeSavedWebhookEvent : WebhookEventBase<MemberTypeSavedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public MemberTypeSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.MemberTypeSaved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(MemberTypeSavedNotification notification)
        => notification.SavedEntities.Select(entity => new DefaultPayloadModel { Id = entity.Key });
}
