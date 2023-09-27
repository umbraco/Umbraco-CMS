using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class StartNodeSelectorFilterCollectionBuilder : OrderedCollectionBuilderBase<StartNodeSelectorFilterCollectionBuilder, StartNodeSelectorFilterCollection, IStartNodeSelectorFilter>
{
    protected override StartNodeSelectorFilterCollectionBuilder This => this;
}
