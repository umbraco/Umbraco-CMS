using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class FilterHandlerCollectionBuilder
    : LazyCollectionBuilderBase<FilterHandlerCollectionBuilder, FilterHandlerCollection, IFilterHandler>
{
    protected override FilterHandlerCollectionBuilder This => this;
}
