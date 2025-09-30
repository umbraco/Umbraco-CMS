using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class DatePickerValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DateTime);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(DateTime);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => ParseDateTimeValue(source);

    internal static DateTime ParseDateTimeValue(object? source)
    {
        if (source is DateTime dateTimeValue)
        {
            return DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Unspecified);
        }

        if (source.TryConvertTo<DateTime>() is { Success: true } attempt)
        {
            return DateTime.SpecifyKind(attempt.Result, DateTimeKind.Unspecified);
        }

        return DateTime.MinValue;
    }
}
