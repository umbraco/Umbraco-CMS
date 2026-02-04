using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides a base class for property value converters that match based on value type.
/// </summary>
public abstract class ValueTypePropertyValueConverterBase : PropertyValueConverterBase
{
    private readonly PropertyEditorCollection _propertyEditors;

    /// <summary>
    ///     Gets the value types supported by this converter.
    /// </summary>
    protected abstract string[] SupportedValueTypes { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueTypePropertyValueConverterBase" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    protected ValueTypePropertyValueConverterBase(PropertyEditorCollection propertyEditors)
        => _propertyEditors = propertyEditors;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => _propertyEditors.TryGet(propertyType.EditorAlias, out IDataEditor? editor)
           && SupportedValueTypes.InvariantContains(editor.GetValueEditor().ValueType);
}
