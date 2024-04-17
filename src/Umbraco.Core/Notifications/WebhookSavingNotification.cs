using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookSavingNotification : SavingNotification<IWebhook>
{
    public WebhookSavingNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }

    public WebhookSavingNotification(IEnumerable<IWebhook> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
