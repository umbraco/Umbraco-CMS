using System.Text.Json.Serialization;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeWithTimeZoneValueConverter"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
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
        return GetPropertyValueType(config);
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
        var sourceStr = source?.ToString();
        if (sourceStr is null || !_jsonSerializer.TryDeserialize(sourceStr, out DateWithTimeZone? dateWithTimeZone))
        {
            return null;
        }

        return dateWithTimeZone;
    }

    public override object? ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel,
        object? inter,
        bool preview)
    {
        DateWithTimeZoneConfiguration? config =
            ConfigurationEditor.ConfigurationAs<DateWithTimeZoneConfiguration>(propertyType.DataType.ConfigurationObject);
        return GetObjectValue(inter, config);
    }

    private static Type GetPropertyValueType(DateWithTimeZoneConfiguration? config) =>
        config?.Format switch
        {
            DateWithTimeZoneFormat.DateOnly => typeof(DateOnly?),
            DateWithTimeZoneFormat.TimeOnly => typeof(TimeOnly?),
            DateWithTimeZoneFormat.DateTime when config.TimeZones?.Mode is not { } mode || mode == DateWithTimeZoneMode.None => typeof(DateTime?),
            _ => typeof(DateTimeOffset?),
        };

    internal static object? GetIntermediateValue(string? source, IJsonSerializer jsonSerializer)
    {
        if (source is null || !jsonSerializer.TryDeserialize(source, out DateWithTimeZone? dateWithTimeZone))
        {
            return null;
        }

        return dateWithTimeZone;
    }

    internal static object? GetObjectValue(object? inter, DateWithTimeZoneConfiguration? configuration)
    {
        Type propertyValueType = GetPropertyValueType(configuration);
        if (inter is null)
        {
            return propertyValueType.GetDefaultValue();
        }

        if (inter is not DateWithTimeZone dateWithTimeZone)
        {
            return propertyValueType.GetDefaultValue();
        }

        if (propertyValueType == typeof(DateOnly?))
        {
            return DateOnly.FromDateTime(dateWithTimeZone.Date.UtcDateTime);
        }

        if (propertyValueType == typeof(TimeOnly?))
        {
            return TimeOnly.FromDateTime(dateWithTimeZone.Date.UtcDateTime);
        }

        if (propertyValueType == typeof(DateTime?))
        {
            return DateTime.SpecifyKind(dateWithTimeZone.Date.UtcDateTime, DateTimeKind.Unspecified);
        }

        return dateWithTimeZone.Date;
    }

    internal static object? GetValue(string? source, DateWithTimeZoneConfiguration? configuration, IJsonSerializer jsonSerializer)
    {
        var intermediateValue = GetIntermediateValue(source, jsonSerializer);
        return GetObjectValue(intermediateValue, configuration);
    }

    internal static string? GetDateValueAsString(object? value) =>
        value switch
        {
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("o"),
            DateOnly dateOnly => dateOnly.ToString("o"),
            TimeOnly timeOnly => timeOnly.ToString("o"),
            DateTime dateTime => dateTime.ToString("o"),
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported type: {value.GetType().FullName}"),
        };

    public class DateWithTimeZone
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; init; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; init; }
    }
}
