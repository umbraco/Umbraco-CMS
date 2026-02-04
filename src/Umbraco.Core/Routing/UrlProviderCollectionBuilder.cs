using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Builds a <see cref="UrlProviderCollection" /> by registering <see cref="IUrlProvider" /> implementations.
/// </summary>
public class UrlProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlProviderCollectionBuilder, UrlProviderCollection, IUrlProvider>
{
    /// <inheritdoc />
    protected override UrlProviderCollectionBuilder This => this;
}
