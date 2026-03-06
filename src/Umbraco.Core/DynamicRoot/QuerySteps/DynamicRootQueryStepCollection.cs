using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     A collection of <see cref="IDynamicRootQueryStep"/> implementations used to filter or traverse the content tree during dynamic root resolution.
/// </summary>
public class DynamicRootQueryStepCollection : BuilderCollectionBase<IDynamicRootQueryStep>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicRootQueryStepCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of query steps.</param>
    public DynamicRootQueryStepCollection(Func<IEnumerable<IDynamicRootQueryStep>> items)
        : base(items)
    {
    }
}
