using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Represents a collection of <see cref="IMediaUrlProvider" /> instances.
/// </summary>
public class MediaUrlProviderCollection : BuilderCollectionBase<IMediaUrlProvider>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaUrlProviderCollection" /> class.
    /// </summary>
    /// <param name="items">A factory function that returns the media URL providers.</param>
    public MediaUrlProviderCollection(Func<IEnumerable<IMediaUrlProvider>> items)
        : base(items)
    {
    }
}
