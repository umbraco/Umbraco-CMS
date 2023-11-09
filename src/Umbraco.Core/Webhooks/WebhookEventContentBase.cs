using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventContentBase<TNotification, TEntity> : WebhookEventBase<TNotification>
    where TNotification : INotification
    where TEntity : IContentBase
{
    protected WebhookEventContentBase(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override async Task ProcessWebhooks(TNotification notification, IEnumerable<Webhook> webhooks, CancellationToken cancellationToken)
    {
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

                await WebhookFiringService.FireAsync(webhook, Alias, ConvertEntityToRequestPayload(entity), cancellationToken);
            }
        }
    }

    protected abstract IEnumerable<TEntity> GetEntitiesFromNotification(TNotification notification);

    protected abstract object? ConvertEntityToRequestPayload(TEntity entity);
}
