using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for property editors with the decimal value type.
/// </summary>
[DefaultValueTypePropertyValueConverter]
public class DecimalValueTypeConverter : ValueTypePropertyValueConverterBase
{
    /// <inheritdoc />
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Decimal };

    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalValueTypeConverter" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editor collection.</param>
    public DecimalValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(decimal);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => DecimalValueConverter.ParseDecimalValue(source); // reuse the value conversion from the default "Umbraco.Decimal" value converter
}
