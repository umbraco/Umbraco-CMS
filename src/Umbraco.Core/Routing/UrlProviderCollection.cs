using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Represents a collection of <see cref="IUrlProvider" /> instances.
/// </summary>
public class UrlProviderCollection : BuilderCollectionBase<IUrlProvider>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UrlProviderCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the URL providers.</param>
    public UrlProviderCollection(Func<IEnumerable<IUrlProvider>> items)
        : base(items)
    {
    }
}
