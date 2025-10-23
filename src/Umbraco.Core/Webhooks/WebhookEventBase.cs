using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventBase<TNotification> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : INotification
{
    private readonly IServerRoleAccessor _serverRoleAccessor;

    private readonly WebhookPayloadProviderCollection _webhookPayloadProviderCollection;

    public abstract string Alias { get; }

    public string EventName { get; set; }

    public string EventType { get; }

    protected IWebhookFiringService WebhookFiringService { get; }

    protected IWebhookService WebhookService { get; }

    protected WebhookSettings WebhookSettings { get; private set; }



    protected WebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : this(
            webhookFiringService,
            webhookService, webhookSettings, serverRoleAccessor,
            StaticServiceProvider.Instance.GetRequiredService<WebhookPayloadProviderCollection>())
    {
    }
    

    protected WebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        WebhookPayloadProviderCollection webhookPayloadProviderCollection)
    {
        WebhookFiringService = webhookFiringService;
        WebhookService = webhookService;
        _serverRoleAccessor = serverRoleAccessor;
        _webhookPayloadProviderCollection = webhookPayloadProviderCollection;

        // assign properties based on the attribute, if it is found
        WebhookEventAttribute? attribute = GetType().GetCustomAttribute<WebhookEventAttribute>(false);

        EventType = attribute?.EventType ?? "Others";
        EventName = attribute?.Name ?? Alias;

        WebhookSettings = webhookSettings.CurrentValue;
        webhookSettings.OnChange(x => WebhookSettings = x);
    }

    /// <summary>
    ///  Process the webhooks for the given notification.
    /// </summary>
    public virtual async Task ProcessWebhooks(TNotification notification, IEnumerable<IWebhook> webhooks, CancellationToken cancellationToken)
    {
        foreach (IWebhook webhook in webhooks)
        {
            if (webhook.Enabled is false)
            {
                continue;
            }

            if (!Uri.TryCreate(webhook.Url, UriKind.Absolute, out Uri? endPoint))
            {
                continue;
            }
            var ctx = new WebhookContext(endPoint, Alias, notification, webhook);

            IWebhookPayloadProvider? provider = GetPayloadProviderProvider(notification, webhook);
            var payload = provider is null
                ? ConvertNotificationToRequestPayload(notification)
                : provider.BuildPayload(ctx);

            await WebhookFiringService.FireAsync(webhook, Alias, payload, cancellationToken);
        }
    }

    protected IWebhookPayloadProvider? GetPayloadProviderProvider(TNotification notification, IWebhook webhook)
    {
        if (!Uri.TryCreate(webhook.Url, UriKind.Absolute, out Uri? endPoint))
        {
            return null;
        }
        var ctx = new WebhookContext(endPoint, Alias, notification, webhook);
        return _webhookPayloadProviderCollection.FirstOrDefault(x => x.CanHandle(ctx));
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

        IEnumerable<IWebhook> webhooks = await WebhookService.GetByAliasAsync(Alias);

        await ProcessWebhooks(notification, webhooks, cancellationToken);
    }

    /// <summary>
    /// Use this method if you wish to change the shape of the object to be serialised
    /// for the JSON webhook payload.
    /// For example excluding sensitive data
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    public virtual object? ConvertNotificationToRequestPayload(TNotification notification)
        => notification;
}
