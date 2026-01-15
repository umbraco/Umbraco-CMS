using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Services.Filters;

/// <summary>
/// Builds an ordered collection of <see cref="IContentTypeFilter"/>.
/// </summary>
public class ContentTypeFilterCollectionBuilder : OrderedCollectionBuilderBase<ContentTypeFilterCollectionBuilder, ContentTypeFilterCollection, IContentTypeFilter>
{
    /// <inheritdoc/>
    protected override ContentTypeFilterCollectionBuilder This => this;
}
