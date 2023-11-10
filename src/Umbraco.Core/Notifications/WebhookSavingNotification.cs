using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookSavingNotification : SavingNotification<Webhook>
{
    public WebhookSavingNotification(Webhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    public WebhookSavingNotification(IEnumerable<Webhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
