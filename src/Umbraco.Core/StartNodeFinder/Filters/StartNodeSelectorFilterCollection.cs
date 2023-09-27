using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class StartNodeSelectorFilterCollection : BuilderCollectionBase<IStartNodeSelectorFilter>
{
    public StartNodeSelectorFilterCollection(Func<IEnumerable<IStartNodeSelectorFilter>> items) : base(items)
    {
    }
}
