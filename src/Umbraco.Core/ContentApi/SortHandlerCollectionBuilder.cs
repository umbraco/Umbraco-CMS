using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class SortHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SortHandlerCollectionBuilder, SortHandlerCollection, ISortHandler>
{
    protected override SortHandlerCollectionBuilder This => this;
}
