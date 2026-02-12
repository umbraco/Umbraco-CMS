using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks;

/// <summary>
/// Abstract base class for webhook events that handles notification processing and webhook firing.
/// </summary>
/// <typeparam name="TNotification">The type of notification this webhook event handles.</typeparam>
public abstract class WebhookEventBase<TNotification> : IWebhookEvent, INotificationAsyncHandler<TNotification>
    where TNotification : INotification
{
    private readonly IServerRoleAccessor _serverRoleAccessor;

    /// <inheritdoc />
    public abstract string Alias { get; }

    /// <inheritdoc />
    public string EventName { get; set; }

    /// <inheritdoc />
    public string EventType { get; }

    /// <summary>
    /// Gets the webhook firing service used to send webhook requests.
    /// </summary>
    protected IWebhookFiringService WebhookFiringService { get; }

    /// <summary>
    /// Gets the webhook service used to retrieve webhook configurations.
    /// </summary>
    protected IWebhookService WebhookService { get; }

    /// <summary>
    /// Gets the current webhook settings configuration.
    /// </summary>
    protected WebhookSettings WebhookSettings { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEventBase{TNotification}"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The service responsible for firing webhooks.</param>
    /// <param name="webhookService">The service for managing webhook configurations.</param>
    /// <param name="webhookSettings">The webhook settings configuration.</param>
    /// <param name="serverRoleAccessor">The server role accessor to determine the current server role.</param>
    protected WebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
    {

        WebhookFiringService = webhookFiringService;
        WebhookService = webhookService;
        _serverRoleAccessor = serverRoleAccessor;

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

            await WebhookFiringService.FireAsync(webhook, Alias, ConvertNotificationToRequestPayload(notification), cancellationToken);
        }
    }

    /// <summary>
    ///  should webhooks fire for this notification.
    /// </summary>
    /// <returns>true if webhooks should be fired.</returns>
    public virtual bool ShouldFireWebhookForNotification(TNotification notificationObject)
        => true;

    /// <inheritdoc />
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
