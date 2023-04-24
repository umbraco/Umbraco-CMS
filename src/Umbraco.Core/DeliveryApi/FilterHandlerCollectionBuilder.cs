using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public class FilterHandlerCollectionBuilder
    : LazyCollectionBuilderBase<FilterHandlerCollectionBuilder, FilterHandlerCollection, IFilterHandler>
{
    protected override FilterHandlerCollectionBuilder This => this;
}
