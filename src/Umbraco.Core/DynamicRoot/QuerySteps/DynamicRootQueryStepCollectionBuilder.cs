using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class DynamicRootQueryStepCollectionBuilder : OrderedCollectionBuilderBase<DynamicRootQueryStepCollectionBuilder, DynamicRootQueryStepCollection, IDynamicRootQueryStep>
{
    protected override DynamicRootQueryStepCollectionBuilder This => this;
}
