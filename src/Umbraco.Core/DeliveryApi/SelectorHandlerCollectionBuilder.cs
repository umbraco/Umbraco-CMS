using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class SelectorHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SelectorHandlerCollectionBuilder, SelectorHandlerCollection, ISelectorHandler>
{
    protected override SelectorHandlerCollectionBuilder This => this;
}
