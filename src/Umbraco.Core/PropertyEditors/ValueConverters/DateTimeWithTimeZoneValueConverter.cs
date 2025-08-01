using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The date time with timezone property value converter.
/// </summary>
[DefaultPropertyValueConverter]
public class DateTimeWithTimeZoneValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public DateTimeWithTimeZoneValueConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        DateWithTimeZoneConfiguration? config =
            ConfigurationEditor.ConfigurationAs<DateWithTimeZoneConfiguration>(propertyType.DataType.ConfigurationObject);
        return config?.Format switch
        {
            DateWithTimeZoneFormat.DateOnly => typeof(DateOnly?),
            DateWithTimeZoneFormat.TimeOnly => typeof(TimeOnly?),
            DateWithTimeZoneFormat.DateTime when config.TimeZones is null => typeof(DateTime?),
            _ => typeof(DateTimeOffset?),
        };
    }

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        object? source,
        bool preview)
    {
        Type propertyValueType = GetPropertyValueType(propertyType);
        var sourceStr = source?.ToString();
        if (sourceStr is null
            || !_jsonSerializer.TryDeserialize(sourceStr, out DateTimeWithTimeZone? dateTimeWithTimezone)
            || !DateTimeOffset.TryParse(dateTimeWithTimezone.Date, out DateTimeOffset dateTimeOffset))
        {
            return propertyValueType.GetDefaultValue();
        }

        if (propertyValueType == typeof(DateOnly?))
        {
            return DateOnly.FromDateTime(dateTimeOffset.DateTime);
        }

        if (propertyValueType == typeof(TimeOnly?))
        {
            return TimeOnly.FromDateTime(dateTimeOffset.DateTime);
        }

        if (propertyValueType == typeof(DateTime?))
        {
            return DateTime.SpecifyKind(dateTimeOffset.DateTime, DateTimeKind.Unspecified);
        }

        return dateTimeOffset;
    }

    internal class DateTimeWithTimeZone
    {
        public required string Date { get; set; }

        public string? TimeZone { get; set; }
    }
}
