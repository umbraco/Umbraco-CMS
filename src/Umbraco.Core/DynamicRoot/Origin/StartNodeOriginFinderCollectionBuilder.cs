using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class StartNodeOriginFinderCollectionBuilder : OrderedCollectionBuilderBase<StartNodeOriginFinderCollectionBuilder, StartNodeOriginFinderCollection, IStartNodeOriginFinder>
{
    protected override StartNodeOriginFinderCollectionBuilder This => this;
}
