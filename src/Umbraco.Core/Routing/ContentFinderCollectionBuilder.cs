using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Builds a <see cref="ContentFinderCollection" /> by registering <see cref="IContentFinder" /> implementations.
/// </summary>
public class ContentFinderCollectionBuilder : OrderedCollectionBuilderBase<ContentFinderCollectionBuilder, ContentFinderCollection, IContentFinder>
{
    /// <inheritdoc />
    protected override ContentFinderCollectionBuilder This => this;
}
