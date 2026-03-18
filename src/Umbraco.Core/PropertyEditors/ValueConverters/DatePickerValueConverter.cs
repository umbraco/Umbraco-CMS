using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for date/time picker properties.
/// </summary>
[DefaultPropertyValueConverter]
public class DatePickerValueConverter : PropertyValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DateTime);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(DateTime);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => ParseDateTimeValue(source);

    /// <summary>
    ///     Parses a source value to a <see cref="DateTime" /> value.
    /// </summary>
    /// <param name="source">The source value to parse.</param>
    /// <returns>The parsed <see cref="DateTime" /> value, or <see cref="DateTime.MinValue" /> if parsing fails.</returns>
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
