using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A collection builder for <see cref="ISelectorHandler"/> implementations.
/// </summary>
public sealed class SelectorHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SelectorHandlerCollectionBuilder, SelectorHandlerCollection, ISelectorHandler>
{
    /// <inheritdoc />
    protected override SelectorHandlerCollectionBuilder This => this;
}
