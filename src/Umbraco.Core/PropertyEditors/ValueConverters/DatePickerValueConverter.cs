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

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => ParseDateTimeValue(source);

    internal static DateTime ParseDateTimeValue(object? source)
    {
        if (source is null)
        {
            return DateTime.MinValue;
        }

        if (source is DateTime dateTimeValue)
        {
            return dateTimeValue;
        }

        Attempt<DateTime> attempt = source.TryConvertTo<DateTime>();
        return attempt.Success
            ? attempt.Result
            : DateTime.MinValue;
    }
}
