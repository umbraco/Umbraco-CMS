using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Webhooks;

public abstract class WebhookEventBase<T> : IWebhookEvent, INotificationAsyncHandler<T>
    where T : INotification
{

    public abstract string EventName { get; }

    public abstract Task HandleAsync(T notification, CancellationToken cancellationToken);
}

