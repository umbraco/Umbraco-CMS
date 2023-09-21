using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookEventCollectionBuilder : OrderedCollectionBuilderBase<WebhookEventCollectionBuilder, WebhookEventCollection, IWebhookEvent>
{
    protected override WebhookEventCollectionBuilder This => this;
}
