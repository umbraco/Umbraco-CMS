using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultValueTypePropertyValueConverter]
public class DateTimeValueTypeConverter : ValueTypePropertyValueConverterBase
{
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Date, ValueTypes.DateTime };

    public DateTimeValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(DateTime);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => DatePickerValueConverter.ParseDateTimeValue(source); // reuse the value conversion from the default "Umbraco.DateTime" value converter
}
