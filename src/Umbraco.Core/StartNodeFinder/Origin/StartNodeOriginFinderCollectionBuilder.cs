using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.StartNodeFinder.Origin;

public class StartNodeOriginFinderCollectionBuilder : OrderedCollectionBuilderBase<StartNodeOriginFinderCollectionBuilder, StartNodeOriginFinderCollection, IStartNodeOriginFinder>
{
    protected override StartNodeOriginFinderCollectionBuilder This => this;
}
