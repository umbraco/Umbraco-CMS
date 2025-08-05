using System.Globalization;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The date time with timezone property value converter.
/// </summary>
[DefaultPropertyValueConverter]
public class DateTimeWithTimeZoneValueConverter : PropertyValueConverterBase
{
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
        DateWithTimeZoneConfiguration? config =
            ConfigurationEditor.ConfigurationAs<DateWithTimeZoneConfiguration>(propertyType.DataType.ConfigurationObject);
        var sourceStr = source?.ToString();
        return GetValue(sourceStr, config);
    }

    private static Type GetPropertyValueType(DateWithTimeZoneConfiguration? config) =>
        config?.Format switch
        {
            DateWithTimeZoneFormat.DateOnly => typeof(DateOnly?),
            DateWithTimeZoneFormat.TimeOnly => typeof(TimeOnly?),
            DateWithTimeZoneFormat.DateTime when config.TimeZones is null => typeof(DateTime?),
            _ => typeof(DateTimeOffset?),
        };

    internal static object? GetValue(string? value, DateWithTimeZoneConfiguration? configuration)
        => GetValue(value is null ? null : JsonNode.Parse(value) as JsonObject, configuration);

    internal static object? GetValue(JsonObject? value, DateWithTimeZoneConfiguration? configuration)
    {
        Type propertyValueType = GetPropertyValueType(configuration);
        if (value is null
            || !DateTimeOffset.TryParse(value["date"]?.GetValue<string>(), null, DateTimeStyles.AssumeUniversal, out DateTimeOffset dateTimeOffset))
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

    internal static string? GetValueAsString(string? value, DateWithTimeZoneConfiguration? configuration)
        => GetValueAsString(value is null ? null : JsonNode.Parse(value) as JsonObject, configuration);

    internal static string? GetValueAsString(JsonObject? value, DateWithTimeZoneConfiguration? configuration)
    {
        var objectValue = GetValue(value, configuration);
        return objectValue switch
        {
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("o"),
            DateOnly dateOnly => dateOnly.ToString("o"),
            TimeOnly timeOnly => timeOnly.ToString("o"),
            DateTime dateTime => dateTime.ToString("o"),
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(objectValue), $"Unsupported type: {objectValue?.GetType().FullName}"),
        };
    }
}
