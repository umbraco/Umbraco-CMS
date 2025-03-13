using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultValueTypePropertyValueConverter]
public class DecimalValueTypeConverter : ValueTypePropertyValueConverterBase
{
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Decimal };

    public DecimalValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(decimal);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => DecimalValueConverter.ParseDecimalValue(source); // reuse the value conversion from the default "Umbraco.Decimal" value converter
}
