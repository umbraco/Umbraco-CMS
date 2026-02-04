using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection builder for <see cref="IFilterHandler"/> implementations.
/// </summary>
public sealed class FilterHandlerCollectionBuilder
    : LazyCollectionBuilderBase<FilterHandlerCollectionBuilder, FilterHandlerCollection, IFilterHandler>
{
    /// <inheritdoc />
    protected override FilterHandlerCollectionBuilder This => this;
}
