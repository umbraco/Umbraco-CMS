using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookPayloadProviderBase<TNotification> : IWebhookPayloadProvider where TNotification : class, INotification
{
    public bool CanHandle(WebhookContext ctx) => ctx.Notification is TNotification n && CanHandle(ctx.Endpoint, ctx.EventAlias, n, ctx.Webhook);

    object IWebhookPayloadProvider.BuildPayload(WebhookContext ctx)
        => BuildPayload((TNotification)ctx.Notification, ctx.Endpoint, ctx.EventAlias, ctx.Webhook);

    protected abstract bool CanHandle(Uri endpoint, string eventAlias, TNotification notification, IWebhook webhook);
    protected abstract object BuildPayload(TNotification notification, Uri endpoint, string eventAlias, IWebhook webhook);
}
