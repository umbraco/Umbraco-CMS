using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public class ContentIndexHandlerCollectionBuilder
    : LazyCollectionBuilderBase<ContentIndexHandlerCollectionBuilder, ContentIndexHandlerCollection, IContentIndexHandler>
{
    protected override ContentIndexHandlerCollectionBuilder This => this;
}
