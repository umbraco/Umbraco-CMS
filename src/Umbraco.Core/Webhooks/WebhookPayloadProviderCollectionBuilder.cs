using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Webhooks;

public class WebhookPayloadProviderCollectionBuilder : LazyCollectionBuilderBase<WebhookPayloadProviderCollectionBuilder, WebhookPayloadProviderCollection, IWebhookPayloadProvider>
{
    protected override WebhookPayloadProviderCollectionBuilder This => this;
}
