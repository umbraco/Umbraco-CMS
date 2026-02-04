using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a builder for the <see cref="PropertyValueConverterCollection"/>.
/// </summary>
public class PropertyValueConverterCollectionBuilder : OrderedCollectionBuilderBase<PropertyValueConverterCollectionBuilder, PropertyValueConverterCollection, IPropertyValueConverter>
{
    /// <inheritdoc />
    protected override PropertyValueConverterCollectionBuilder This => this;
}
