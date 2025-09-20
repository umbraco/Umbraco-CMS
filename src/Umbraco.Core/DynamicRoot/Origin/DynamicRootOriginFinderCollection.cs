using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class DynamicRootOriginFinderCollection : BuilderCollectionBase<IDynamicRootOriginFinder>
{
    public DynamicRootOriginFinderCollection(Func<IEnumerable<IDynamicRootOriginFinder>> items)
        : base(items)
    {
    }
}
