using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Webhooks;

/// <summary>
/// A collection of <see cref="IWebhookEvent"/> instances registered in the system.
/// </summary>
public class WebhookEventCollection : BuilderCollectionBase<IWebhookEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEventCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of webhook events.</param>
    public WebhookEventCollection(Func<IEnumerable<IWebhookEvent>> items) : base(items)
    {
    }
}
