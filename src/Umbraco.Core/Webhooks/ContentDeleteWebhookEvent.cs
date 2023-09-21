using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Webhooks;

public class ContentDeleteWebhookEvent : WebhookEventBase<ContentDeletedNotification>
{
    public override string EventName => "ContentPublish";

    public override async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        // Implement your handling logic for the ContentPublish event
        // You can access properties of the ContentSavedNotification here
        await Task.Yield(); // Replace with your actual async logic
    }
}
