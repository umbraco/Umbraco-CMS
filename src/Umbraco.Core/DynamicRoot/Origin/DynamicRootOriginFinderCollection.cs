using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DynamicRoot.Origin;

/// <summary>
///     A collection of <see cref="IDynamicRootOriginFinder"/> implementations used to find the origin for dynamic root resolution.
/// </summary>
public class DynamicRootOriginFinderCollection : BuilderCollectionBase<IDynamicRootOriginFinder>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicRootOriginFinderCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of origin finders.</param>
    public DynamicRootOriginFinderCollection(Func<IEnumerable<IDynamicRootOriginFinder>> items)
        : base(items)
    {
    }
}
