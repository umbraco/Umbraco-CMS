using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookSavedNotification : SavedNotification<Webhook>
{
    public WebhookSavedNotification(Webhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    public WebhookSavedNotification(IEnumerable<Webhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
