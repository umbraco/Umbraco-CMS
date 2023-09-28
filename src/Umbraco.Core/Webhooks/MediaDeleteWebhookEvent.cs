using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaDeleteWebhookEvent : WebhookEventBase<MediaDeletedNotification>
{
    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webHookService;

    public MediaDeleteWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService)
    {
        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
        EventName = Constants.WebhookEvents.MediaDelete;
    }

    public override async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<Webhook> webhooks = await _webHookService.GetByEventNameAsync(EventName);

        foreach (Webhook webhook in webhooks)
        {
            foreach (IMedia media in notification.DeletedEntities)
            {
                if (webhook.EntityKeys.Contains(media.ContentType.Key) is false)
                {
                    continue;
                }

                HttpResponseMessage response = await _webhookFiringService.Fire(webhook.Url, media);

                // TODO: Implement logging depending on response here
                if (response.IsSuccessStatusCode)
                {

                }
            }
        }
    }
}
