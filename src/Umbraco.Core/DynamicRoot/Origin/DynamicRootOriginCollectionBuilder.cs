using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class DynamicRootOriginFinderCollectionBuilder : OrderedCollectionBuilderBase<DynamicRootOriginFinderCollectionBuilder, DynamicRootOriginFinderCollection, IDynamicRootOriginFinder>
{
    protected override DynamicRootOriginFinderCollectionBuilder This => this;
}
