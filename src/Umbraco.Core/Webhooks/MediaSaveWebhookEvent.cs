using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Webhooks;

public class MediaSaveWebhookEvent : WebhookEventBase<MediaSavedNotification>
{
    public override string EventName => "MediaSave";

    public override Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken) => throw new NotImplementedException();
}
