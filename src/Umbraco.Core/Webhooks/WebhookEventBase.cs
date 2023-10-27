using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventBase<TNotification, TEntity> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : INotification
    where TEntity : IContentBase
{
    private readonly IWebhookFiringService _webhookFiringService;
    private readonly IWebHookService _webHookService;
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookLogFactory _webhookLogFactory;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private WebhookSettings _webhookSettings;

    protected WebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IWebhookLogService webhookLogService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IWebhookLogFactory webhookLogFactory,
        IServerRoleAccessor serverRoleAccessor,
        string eventName)
    {
        _webhookFiringService = webhookFiringService;
        _webHookService = webHookService;
        _webhookLogService = webhookLogService;
        _webhookLogFactory = webhookLogFactory;
        _serverRoleAccessor = serverRoleAccessor;
        EventName = eventName;
        _webhookSettings = webhookSettings.CurrentValue;
        webhookSettings.OnChange(x => _webhookSettings = x);
    }

    public string EventName { get; set; }

    public virtual async Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
    {
        if (_serverRoleAccessor.CurrentServerRole is not ServerRole.Single && _serverRoleAccessor.CurrentServerRole is not ServerRole.SchedulingPublisher)
        {
            return;
        }

        if (_webhookSettings.Enabled is false)
        {
            return;
        }

        IEnumerable<Webhook> webhooks = await _webHookService.GetByEventNameAsync(EventName);

        foreach (Webhook webhook in webhooks)
        {
            if (!webhook.Enabled)
            {
                continue;
            }

            foreach (TEntity entity in GetEntitiesFromNotification(notification))
            {
                if (webhook.ContentTypeKeys.Any() && !webhook.ContentTypeKeys.Contains(entity.ContentType.Key))
                {
                    continue;
                }

                WebhookResponseModel response = await _webhookFiringService.Fire(webhook, EventName, entity);

                WebhookLog log = await _webhookLogFactory.CreateAsync(EventName, response, webhook, cancellationToken);
                await _webhookLogService.CreateAsync(log);
            }
        }
    }

    protected abstract IEnumerable<TEntity> GetEntitiesFromNotification(TNotification notification);
}
