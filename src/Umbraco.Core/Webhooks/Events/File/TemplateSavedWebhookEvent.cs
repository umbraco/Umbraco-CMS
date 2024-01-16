using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
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

    public override string Alias => "templateSaved";

    public override object? ConvertNotificationToRequestPayload(TemplateSavedNotification notification)
    {
        // Create a new anonymous object with the properties we want
        return new
        {
            notification.CreateTemplateForContentType,
            notification.ContentTypeAlias,
            notification.SavedEntities
        };
    }
}
