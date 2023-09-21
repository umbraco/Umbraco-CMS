using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentUnpublishWebhookEvent : WebhookEventBase<ContentUnpublishedNotification>
{
    public ContentUnpublishWebhookEvent() => EventName = "ContentUnpublish";

    public override async Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        await Task.Yield(); // Replace with your actual async logic
    }
}
