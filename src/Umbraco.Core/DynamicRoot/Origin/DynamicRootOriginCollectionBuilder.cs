using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class DynamicRootOriginCollectionBuilder : OrderedCollectionBuilderBase<DynamicRootOriginCollectionBuilder, DynamicRootOriginCollection, IDynamicRootOrigin>
{
    protected override DynamicRootOriginCollectionBuilder This => this;
}
