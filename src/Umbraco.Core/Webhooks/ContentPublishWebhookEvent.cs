using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentPublishWebhookEvent : WebhookEventBase<ContentPublishedNotification, IContent>
{
    public ContentPublishWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
        : base(webhookFiringService, webHookService, webhookLogRepository, coreScopeProvider, Constants.WebhookEvents.ContentPublish)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) => notification.PublishedEntities;
}
