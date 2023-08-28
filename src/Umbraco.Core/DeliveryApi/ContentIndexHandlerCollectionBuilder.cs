using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ContentIndexHandlerCollectionBuilder
    : LazyCollectionBuilderBase<ContentIndexHandlerCollectionBuilder, ContentIndexHandlerCollection, IContentIndexHandler>
{
    protected override ContentIndexHandlerCollectionBuilder This => this;
}
