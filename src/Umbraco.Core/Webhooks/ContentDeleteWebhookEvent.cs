using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentDeleteWebhookEvent : WebhookEventBase<ContentDeletedNotification, IContent>
{
    public ContentDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
        : base(webhookFiringService, webHookService, webhookLogRepository, coreScopeProvider, Constants.WebhookEvents.ContentDelete)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedNotification notification) => notification.DeletedEntities;
}
