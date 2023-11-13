using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookDeletingNotification : DeletingNotification<Webhook>
{
    public WebhookDeletingNotification(Webhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    public WebhookDeletingNotification(IEnumerable<Webhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
