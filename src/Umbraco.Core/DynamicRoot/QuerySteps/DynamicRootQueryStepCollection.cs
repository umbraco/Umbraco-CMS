using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class DynamicRootQueryStepCollection : BuilderCollectionBase<IDynamicRootQueryStep>
{
    public DynamicRootQueryStepCollection(Func<IEnumerable<IDynamicRootQueryStep>> items)
        : base(items)
    {
    }
}
