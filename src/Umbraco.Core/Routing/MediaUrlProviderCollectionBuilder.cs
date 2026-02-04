using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Builds a <see cref="MediaUrlProviderCollection" /> by registering <see cref="IMediaUrlProvider" /> implementations.
/// </summary>
public class MediaUrlProviderCollectionBuilder : OrderedCollectionBuilderBase<MediaUrlProviderCollectionBuilder, MediaUrlProviderCollection, IMediaUrlProvider>
{
    /// <inheritdoc />
    protected override MediaUrlProviderCollectionBuilder This => this;
}
