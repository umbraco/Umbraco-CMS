using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class StartNodeOriginFinderCollection : BuilderCollectionBase<IDynamicRootOrigin>
{
    public StartNodeOriginFinderCollection(Func<IEnumerable<IDynamicRootOrigin>> items) : base(items)
    {
    }
}
