using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentPublishWebhookEvent : WebhookEventBase<ContentPublishedNotification>
{
    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webHookService;

    public ContentPublishWebhookEvent(IWebhookFiringService webhookFiringService, IWebHookService webHookService)
    {
        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
        EventName = Constants.WebhookEvents.ContentPublish;
    }

    public override async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<Webhook> all = await _webHookService.GetAllAsync();
        IEnumerable<Webhook> webhooks = all.Where(x => x.Events.Contains(EventName));

        foreach (Webhook webhook in webhooks)
        {
            foreach (IContent content in notification.PublishedEntities)
            {
                if (webhook.EntityKeys.Contains(content.ContentType.Key) is false)
                {
                    continue;
                }

                HttpResponseMessage response = await _webhookFiringService.Fire(webhook.Url, content);

                // TODO: Implement logging depending on response here
                if (response.IsSuccessStatusCode)
                {

                }
            }
        }
    }
}
