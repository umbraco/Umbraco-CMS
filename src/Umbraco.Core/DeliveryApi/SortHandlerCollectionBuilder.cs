using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class SortHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SortHandlerCollectionBuilder, SortHandlerCollection, ISortHandler>
{
    protected override SortHandlerCollectionBuilder This => this;
}
