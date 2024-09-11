using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookEventCollection : BuilderCollectionBase<IWebhookEvent>
{
    public WebhookEventCollection(Func<IEnumerable<IWebhookEvent>> items) : base(items)
    {
    }
}
