using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentUnpublishWebhookEvent : WebhookEventBase<ContentUnpublishedNotification>
{
    private readonly IWebHookService _webHookService;
    private readonly IWebhookFiringService _webhookFiringService;
    public ContentUnpublishWebhookEvent(IWebHookService webHookService, IWebhookFiringService webhookFiringService)
    {
        _webHookService = webHookService;
        _webhookFiringService = webhookFiringService;
        EventName = Constants.WebhookEvents.ContentUnpublish;
    }

    public override async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<Webhook> webhooks = await _webHookService.GetByEventNameAsync(EventName);

        foreach (Webhook webhook in webhooks)
        {
            foreach (IContent content in notification.UnpublishedEntities)
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
