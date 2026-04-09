using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Builds a collection of <see cref="IUrlSegmentProvider"/> instances.
/// </summary>
/// <remarks>
///     URL segment providers are ordered and determine how content URLs are generated.
/// </remarks>
public class UrlSegmentProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlSegmentProviderCollectionBuilder, UrlSegmentProviderCollection, IUrlSegmentProvider>
{
    /// <inheritdoc />
    protected override UrlSegmentProviderCollectionBuilder This => this;
}
