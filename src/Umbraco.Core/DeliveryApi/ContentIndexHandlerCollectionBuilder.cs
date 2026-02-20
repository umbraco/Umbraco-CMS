using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection builder for <see cref="IContentIndexHandler"/> implementations.
/// </summary>
public sealed class ContentIndexHandlerCollectionBuilder
    : LazyCollectionBuilderBase<ContentIndexHandlerCollectionBuilder, ContentIndexHandlerCollection, IContentIndexHandler>
{
    /// <inheritdoc />
    protected override ContentIndexHandlerCollectionBuilder This => this;
}
