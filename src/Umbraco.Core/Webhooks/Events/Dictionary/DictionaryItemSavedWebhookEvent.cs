using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Dictionary Item Saved")]
public class DictionaryItemSavedWebhookEvent : WebhookEventBase<DictionaryItemSavedNotification>
{
    public DictionaryItemSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => "dictionaryItemSaved";

    public override object? ConvertNotificationToRequestPayload(DictionaryItemSavedNotification notification)
        => notification.SavedEntities;
}
