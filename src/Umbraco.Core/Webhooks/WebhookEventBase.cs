using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventBase<TNotification, TEntity> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : INotification
    where TEntity : IContentBase
{

    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webHookService;
    private readonly IWebhookLogService _webhookLogService;

    protected WebhookEventBase(IWebhookFiringService webhookFiringService, IWebHookService webHookService, IWebhookLogService webhookLogService, string eventName)
    {
        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
        _webhookLogService = webhookLogService;
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

                WebhookResponseModel response = await _webhookFiringService.Fire(webhook.Url, EventName, entity);

                var log = new WebhookLog
                {
                    Date = DateTime.UtcNow,
                    EventName = EventName,
                    RequestBody = await response.HttpResponseMessage.RequestMessage!.Content!.ReadAsStringAsync(cancellationToken),
                    ResponseBody = await response.HttpResponseMessage.Content.ReadAsStringAsync(cancellationToken),
                    StatusCode = response.HttpResponseMessage.StatusCode.ToString(),
                    RetryCount = response.RetryCount,
                    Key = Guid.NewGuid(),
                    Url = webhook.Url,
                    ResponseHeaders = response.HttpResponseMessage.Headers.ToString(),
                    RequestHeaders = response.HttpResponseMessage.RequestMessage.Headers.ToString(),
                };
                await _webhookLogService.CreateAsync(log);
            }
        }
    }

    protected abstract IEnumerable<TEntity> GetEntitiesFromNotification(TNotification notification);
}

