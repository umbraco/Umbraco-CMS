using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Member Type Saved")]
public class LegacyMemberTypeSavedWebhookEvent : WebhookEventBase<MemberTypeSavedNotification>
{
    public LegacyMemberTypeSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MemberTypeSaved;

    public override object? ConvertNotificationToRequestPayload(MemberTypeSavedNotification notification) =>
        notification.SavedEntities;
}
