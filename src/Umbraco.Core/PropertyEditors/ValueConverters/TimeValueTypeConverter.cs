using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultValueTypePropertyValueConverter]
public class TimeValueTypeConverter : ValueTypePropertyValueConverterBase
{
    protected override string[] SupportedValueTypes => new[] { ValueTypes.Time };

    public TimeValueTypeConverter(PropertyEditorCollection propertyEditors)
        : base(propertyEditors)
    {
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(TimeSpan);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source is DateTime dateTimeValue ? dateTimeValue.ToUniversalTime().TimeOfDay : null;
}
