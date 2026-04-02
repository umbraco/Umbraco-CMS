using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for property editors with the text or string value type.
/// </summary>
[DefaultValueTypePropertyValueConverter]
public class TextStringValueTypeConverter : ValueTypePropertyValueConverterBase
{
    /// <inheritdoc />
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Text, ValueTypes.String };

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextStringValueTypeConverter" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    public TextStringValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(string);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source as string;
}
