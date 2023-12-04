using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Partial View Saved")]
public class PartialViewSavedWebhookEvent : WebhookEventBase<PartialViewSavedNotification>
{
    public PartialViewSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => "partialViewSaved";

    public override object? ConvertNotificationToRequestPayload(PartialViewSavedNotification notification) =>
        notification.SavedEntities;
}
