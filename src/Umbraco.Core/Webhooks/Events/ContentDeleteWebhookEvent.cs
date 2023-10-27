using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks.Events;

public class ContentDeleteWebhookEvent : WebhookEventBase<ContentDeletedNotification, IContent>
{
    public ContentDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService, IOptionsMonitor<WebhookSettings> webhookSettings, IWebhookLogFactory webhookLogFactory)
        : base(webhookFiringService, webHookService, webhookLogService, webhookSettings, webhookLogFactory, Constants.WebhookEvents.ContentDelete)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedNotification notification) => notification.DeletedEntities;
}
