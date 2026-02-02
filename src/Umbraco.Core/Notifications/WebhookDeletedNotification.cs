using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class WebhookDeletedNotification : DeletedNotification<IWebhook>
{
    public WebhookDeletedNotification(IWebhook target, EventMessages messages)
        : base(target, messages)
    {
    }
}
