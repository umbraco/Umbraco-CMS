using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Template Saved")]
public class TemplateSavedWebhookEvent : WebhookEventBase<TemplateSavedNotification>
{
    public TemplateSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.TemplateSaved;

    public override object? ConvertNotificationToRequestPayload(TemplateSavedNotification notification)
        => notification.SavedEntities.Select(entity => new
        {
            Id = entity.Key,
            notification.ContentTypeAlias,
        });
}
