using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventBase<TNotification, TEntity> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : INotification
    where TEntity : IContentBase
{

    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webHookService;

    protected WebhookEventBase(IWebhookFiringService webhookFiringService, IWebHookService webHookService, string eventName)
    {
        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
        EventName = eventName;
    }

    public string EventName { get; set; }

    public async Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<Webhook> webhooks = await _webHookService.GetByEventNameAsync(EventName);

        foreach (Webhook webhook in webhooks)
        {
            foreach (TEntity entity in GetEntitiesFromNotification(notification))
            {
                if (!webhook.EntityKeys.Contains(entity.ContentType.Key))
                {
                    continue;
                }

                HttpResponseMessage response = await _webhookFiringService.Fire(webhook.Url, EventName, entity);

                // TODO: Implement logging depending on response here
                if (response.IsSuccessStatusCode)
                {
                    // Handle success
                }
            }
        }
    }

    protected abstract IEnumerable<TEntity> GetEntitiesFromNotification(TNotification notification);
}

