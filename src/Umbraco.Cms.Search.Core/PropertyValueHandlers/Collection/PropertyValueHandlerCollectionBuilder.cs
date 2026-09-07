using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

/// <summary>
/// Builds the collection of registered <see cref="IPropertyValueHandler"/> implementations.
/// </summary>
public sealed class PropertyValueHandlerCollectionBuilder
    : LazyCollectionBuilderBase<PropertyValueHandlerCollectionBuilder, PropertyValueHandlerCollection, IPropertyValueHandler>
{
    /// <inheritdoc />
    protected override PropertyValueHandlerCollectionBuilder This => this;
}
