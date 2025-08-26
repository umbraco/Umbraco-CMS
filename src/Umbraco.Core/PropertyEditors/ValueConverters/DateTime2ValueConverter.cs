using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The date time with timezone property value converter.
/// </summary>
[DefaultPropertyValueConverter]
public class DateTime2ValueConverter : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTime2ValueConverter"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public DateTime2ValueConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DateTime2);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        DateTime2Configuration? config =
            ConfigurationEditor.ConfigurationAs<DateTime2Configuration>(propertyType.DataType.ConfigurationObject);
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
        if (sourceStr is null || !_jsonSerializer.TryDeserialize(sourceStr, out DateTime2? dateTime))
        {
            return null;
        }

        return dateTime;
    }

    public override object? ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel,
        object? inter,
        bool preview)
    {
        DateTime2Configuration? config =
            ConfigurationEditor.ConfigurationAs<DateTime2Configuration>(propertyType.DataType.ConfigurationObject);
        return GetObjectValue(inter, config);
    }

    private static Type GetPropertyValueType(DateTime2Configuration? config) =>
        config?.Format switch
        {
            DateTime2Configuration.DateTimeFormat.DateOnly => typeof(DateOnly?),
            DateTime2Configuration.DateTimeFormat.TimeOnly => typeof(TimeOnly?),
            DateTime2Configuration.DateTimeFormat.DateTime when config.TimeZones?.Mode is not { } mode || mode == DateTime2Configuration.TimeZoneMode.None => typeof(DateTime?),
            _ => typeof(DateTimeOffset?),
        };

    internal static object? GetIntermediateValue(string? source, IJsonSerializer jsonSerializer)
    {
        if (source is null || !jsonSerializer.TryDeserialize(source, out DateTime2? dateTime))
        {
            return null;
        }

        return dateTime;
    }

    internal static object? GetObjectValue(object? inter, DateTime2Configuration? configuration)
    {
        Type propertyValueType = GetPropertyValueType(configuration);
        if (inter is null)
        {
            return propertyValueType.GetDefaultValue();
        }

        if (inter is not DateTime2 dateTime)
        {
            return propertyValueType.GetDefaultValue();
        }

        if (propertyValueType == typeof(DateOnly?))
        {
            return DateOnly.FromDateTime(dateTime.Date.UtcDateTime);
        }

        if (propertyValueType == typeof(TimeOnly?))
        {
            return TimeOnly.FromDateTime(dateTime.Date.UtcDateTime);
        }

        if (propertyValueType == typeof(DateTime?))
        {
            return DateTime.SpecifyKind(dateTime.Date.UtcDateTime, DateTimeKind.Unspecified);
        }

        return dateTime.Date;
    }

    internal static object? GetValue(string? source, DateTime2Configuration? configuration, IJsonSerializer jsonSerializer)
    {
        var intermediateValue = GetIntermediateValue(source, jsonSerializer);
        return GetObjectValue(intermediateValue, configuration);
    }

    internal static string? GetDateValueAsString(object? value) =>
        value switch
        {
            DateTimeOffset or DateOnly or TimeOnly or DateTime => $"{value:O}",
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported type: {value.GetType().FullName}"),
        };

    public class DateTime2
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; init; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; init; }
    }
}
