using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookSavedNotification : SavedNotification<IWebhook>
{
    public WebhookSavedNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    public WebhookSavedNotification(IEnumerable<IWebhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
