using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Web.UI;

public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<WebhookPayloadProviderCollectionBuilder>().Add<MyCustomWebhookPayloadProvider>();
    }
}
