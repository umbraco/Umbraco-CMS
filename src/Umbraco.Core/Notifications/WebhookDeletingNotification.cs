using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookDeletingNotification : DeletingNotification<IWebhook>
{
    public WebhookDeletingNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    public WebhookDeletingNotification(IEnumerable<IWebhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
