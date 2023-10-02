using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentUnpublishWebhookEvent : WebhookEventBase<ContentUnpublishedNotification, IContent>
{
    public ContentUnpublishWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService)
        : base(webhookFiringService, webHookService, webhookLogService, Constants.WebhookEvents.ContentUnpublish)
    {
    }

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentUnpublishedNotification notification) => throw new NotImplementedException();
}
