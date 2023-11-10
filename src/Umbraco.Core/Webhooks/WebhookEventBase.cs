using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventBase<TNotification> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : INotification
{
    private readonly IServerRoleAccessor _serverRoleAccessor;

    /// <InheritDoc />
    public string EventName { get; set; }

    protected IWebhookFiringService WebhookFiringService { get; }
    protected IWebhookService WebHookService { get; }
    protected WebhookSettings WebhookSettings { get; private set; }

    protected WebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        string eventName)
    {
        EventName = eventName;

        WebhookFiringService = webhookFiringService;
        WebHookService = webHookService;
        _serverRoleAccessor = serverRoleAccessor;

        WebhookSettings = webhookSettings.CurrentValue;
        webhookSettings.OnChange(x => WebhookSettings = x);
    }

    /// <summary>
    ///  Process the webhooks for the given notification.
    /// </summary>
    public virtual async Task ProcessWebhooks(TNotification notification, IEnumerable<Webhook> webhooks, CancellationToken cancellationToken)
    {
        foreach (Webhook webhook in webhooks)
        {
            if (webhook.Enabled is false)
            {
                continue;
            }

            await WebhookFiringService.FireAsync(webhook, EventName, notification, cancellationToken);
        }
    }

    /// <summary>
    ///  should webhooks fire for this notification.
    /// </summary>
    /// <returns>true if webhooks should be fired.</returns>
    public virtual bool ShouldFireWebhookForNotification(TNotification notificationObject)
        => true;

    public async Task HandleAsync(TNotification notification, CancellationToken cancellationToken)
    {
        if (WebhookSettings.Enabled is false)
        {
            return;
        }

        if (_serverRoleAccessor.CurrentServerRole is not ServerRole.Single
            && _serverRoleAccessor.CurrentServerRole is not ServerRole.SchedulingPublisher)
        {
            return;
        }

        if (ShouldFireWebhookForNotification(notification) is false)
        {
            return;
        }

        IEnumerable<Webhook> webhooks = await WebHookService.GetByEventNameAsync(EventName);

        await ProcessWebhooks(notification, webhooks, cancellationToken);
    }
}
