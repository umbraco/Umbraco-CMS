using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentUnpublishWebhookEvent : WebhookEventBase<ContentUnpublishedNotification, IContent>
{
    public ContentUnpublishWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
        : base(webhookFiringService, webHookService, webhookLogRepository, coreScopeProvider, Constants.WebhookEvents.ContentUnpublish)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentUnpublishedNotification notification) => throw new NotImplementedException();
}
