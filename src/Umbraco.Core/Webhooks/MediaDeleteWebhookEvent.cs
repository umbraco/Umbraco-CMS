using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaDeleteWebhookEvent : WebhookEventBase<MediaDeletedNotification>
{
    public MediaDeleteWebhookEvent() => EventName = Constants.WebhookEvents.MediaDelete;

    public override Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken) => throw new NotImplementedException();
}
