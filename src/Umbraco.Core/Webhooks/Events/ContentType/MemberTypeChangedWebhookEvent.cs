using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a member type is changed.
/// </summary>
[WebhookEvent("Member Type Changed")]
public class MemberTypeChangedWebhookEvent : WebhookEventBase<MemberTypeChangedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeChangedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public MemberTypeChangedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.MemberTypeChanged;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(MemberTypeChangedNotification notification)
        => notification.Changes.Select(contentTypeChange => new
        {
            Id = contentTypeChange.Item.Key,
            ContentTypeChange = contentTypeChange.ChangeTypes,
        });
}
