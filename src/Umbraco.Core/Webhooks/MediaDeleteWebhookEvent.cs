using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaDeleteWebhookEvent : WebhookEventBase<MediaDeletedNotification, IMedia>
{
    public MediaDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogRepository webhookLogRepository, ICoreScopeProvider coreScopeProvider)
        : base(webhookFiringService, webHookService, webhookLogRepository, coreScopeProvider, Constants.WebhookEvents.MediaDelete)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaDeletedNotification notification) => notification.DeletedEntities;
}
