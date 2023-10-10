using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaDeleteWebhookEvent : WebhookEventBase<MediaDeletedNotification, IMedia>
{
    public MediaDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService, IOptionsMonitor<WebhookSettings> webhookSettings)
        : base(webhookFiringService, webHookService, webhookLogService, webhookSettings, Constants.WebhookEvents.MediaDelete)
    {
    }

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaDeletedNotification notification) => notification.DeletedEntities;
}
