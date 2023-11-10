using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.UI;

public class WebhookNotificationsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<WebhookSavingNotification, WebhookNotificationHandler>();
        builder.AddNotificationHandler<WebhookSavedNotification, WebhookNotificationHandler>();
        builder.AddNotificationHandler<WebhookDeletingNotification, WebhookNotificationHandler>();
        builder.AddNotificationHandler<WebhookDeletedNotification, WebhookNotificationHandler>();
    }
}

public class WebhookNotificationHandler :
    INotificationHandler<WebhookSavingNotification>,
    INotificationHandler<WebhookSavedNotification>,
    INotificationHandler<WebhookDeletingNotification>,
    INotificationHandler<WebhookDeletedNotification>
{
    private readonly ILogger<WebhookNotificationHandler> _logger;

    public WebhookNotificationHandler(ILogger<WebhookNotificationHandler> logger) => _logger = logger;

    public void Handle(WebhookSavingNotification notification)
    {
        foreach (Webhook savedEntity in notification.SavedEntities)
        {
            _logger.LogInformation($"Saving webhook: {savedEntity.Url}");
        }
    }

    public void Handle(WebhookSavedNotification notification)
    {
        foreach (Webhook savedEntity in notification.SavedEntities)
        {
            _logger.LogInformation($"Saved webhook: {savedEntity.Url}");
        }
    }

    public void Handle(WebhookDeletingNotification notification)
    {
        notification.CancelOperation(new EventMessage("", ""));
        foreach (Webhook savedEntity in notification.DeletedEntities)
        {
            _logger.LogInformation($"Deleting webhook: {savedEntity.Url}");
        }
    }

    public void Handle(WebhookDeletedNotification notification)
    {
        foreach (Webhook savedEntity in notification.DeletedEntities)
        {
            _logger.LogInformation($"Deleted webhook: {savedEntity.Url}");
        }
    }
}
