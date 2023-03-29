using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public class SelectorHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SelectorHandlerCollectionBuilder, SelectorHandlerCollection, ISelectorHandler>
{
    protected override SelectorHandlerCollectionBuilder This => this;
}
