using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection builder for <see cref="ISortHandler"/> implementations.
/// </summary>
public sealed class SortHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SortHandlerCollectionBuilder, SortHandlerCollection, ISortHandler>
{
    /// <inheritdoc />
    protected override SortHandlerCollectionBuilder This => this;
}
