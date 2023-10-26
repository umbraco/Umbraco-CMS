using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

public class DynamicRootOriginCollection : BuilderCollectionBase<IDynamicRootOrigin>
{
    public DynamicRootOriginCollection(Func<IEnumerable<IDynamicRootOrigin>> items) : base(items)
    {
    }
}
