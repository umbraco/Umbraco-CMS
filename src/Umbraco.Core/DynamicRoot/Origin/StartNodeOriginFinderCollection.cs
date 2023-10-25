using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class StartNodeOriginFinderCollection : BuilderCollectionBase<IStartNodeOriginFinder>
{
    public StartNodeOriginFinderCollection(Func<IEnumerable<IStartNodeOriginFinder>> items) : base(items)
    {
    }
}
