using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks;

/// <summary>
/// Abstract base class for webhook events that handle content-based notifications,
/// providing filtering by content type and per-entity webhook firing.
/// </summary>
/// <typeparam name="TNotification">The type of notification this webhook event handles.</typeparam>
/// <typeparam name="TEntity">The type of content entity associated with the notification.</typeparam>
public abstract class WebhookEventContentBase<TNotification, TEntity> : WebhookEventBase<TNotification>
    where TNotification : INotification
    where TEntity : IContentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEventContentBase{TNotification, TEntity}"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The service responsible for firing webhooks.</param>
    /// <param name="webhookService">The service for managing webhook configurations.</param>
    /// <param name="webhookSettings">The webhook settings configuration.</param>
    /// <param name="serverRoleAccessor">The server role accessor to determine the current server role.</param>
    protected WebhookEventContentBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override async Task ProcessWebhooks(TNotification notification, IEnumerable<IWebhook> webhooks, CancellationToken cancellationToken)
    {
        foreach (IWebhook webhook in webhooks)
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

                await WebhookFiringService.FireAsync(webhook, Alias, ConvertEntityToRequestPayload(entity), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Extracts the content entities from the notification.
    /// </summary>
    /// <param name="notification">The notification containing the entities.</param>
    /// <returns>An enumerable of content entities from the notification.</returns>
    protected abstract IEnumerable<TEntity> GetEntitiesFromNotification(TNotification notification);

    /// <summary>
    /// Converts a content entity to the webhook request payload.
    /// </summary>
    /// <param name="entity">The content entity to convert.</param>
    /// <returns>The payload object to be sent in the webhook request.</returns>
    protected abstract object? ConvertEntityToRequestPayload(TEntity entity);
}
