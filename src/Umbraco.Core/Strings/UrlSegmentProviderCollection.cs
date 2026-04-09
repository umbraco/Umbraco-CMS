using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Represents a collection of <see cref="IUrlSegmentProvider"/> instances.
/// </summary>
/// <remarks>
///     This collection is built by <see cref="UrlSegmentProviderCollectionBuilder"/> and provides
///     URL segment providers in priority order.
/// </remarks>
public class UrlSegmentProviderCollection : BuilderCollectionBase<IUrlSegmentProvider>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UrlSegmentProviderCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of URL segment providers.</param>
    public UrlSegmentProviderCollection(Func<IEnumerable<IUrlSegmentProvider>> items)
        : base(items)
    {
    }
}
