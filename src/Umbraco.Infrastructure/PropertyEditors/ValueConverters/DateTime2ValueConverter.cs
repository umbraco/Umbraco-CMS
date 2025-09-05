using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The date time with timezone property value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
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
        => GetIntermediateFromSource(source, _jsonSerializer);

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel,
        object? inter,
        bool preview)
    {
        DateTime2Configuration? config =
            ConfigurationEditor.ConfigurationAs<DateTime2Configuration>(propertyType.DataType.ConfigurationObject);
        return GetObjectFromIntermediate(inter, config);
    }

    /// <summary>
    /// Converts the source value to an intermediate representation.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <returns>The intermediate representation.</returns>
    internal static DateTime2Dto? GetIntermediateFromSource(object? source, IJsonSerializer jsonSerializer) =>
        source switch
        {
            // This DateTime check is for compatibility with the "deprecated" `Umbraco.DateTime`.
            // Once that is removed, this can be removed too.
            DateTime dateTime => new DateTime2Dto { Date = new DateTimeOffset(dateTime, TimeSpan.Zero) },
            string sourceStr => jsonSerializer.TryDeserialize(sourceStr, out DateTime2Dto? dateTime2) ? dateTime2 : null,
            _ => null,
        };

    /// <summary>
    /// Converts the intermediate value to the appropriate type based on the configuration.
    /// </summary>
    /// <param name="inter">The intermediate value.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The converted value.</returns>
    internal static object? GetObjectFromIntermediate(object? inter, DateTime2Configuration? configuration)
    {
        Type propertyValueType = GetPropertyValueType(configuration);
        if (inter is not DateTime2Dto dateTime2)
        {
            return propertyValueType.GetDefaultValue();
        }

        return propertyValueType switch
        {
            _ when propertyValueType == typeof(DateOnly?)
                => DateOnly.FromDateTime(dateTime2.Date.UtcDateTime),
            _ when propertyValueType == typeof(TimeOnly?)
                => TimeOnly.FromDateTime(dateTime2.Date.UtcDateTime),
            _ when propertyValueType == typeof(DateTime?)
                => DateTime.SpecifyKind(dateTime2.Date.UtcDateTime, DateTimeKind.Unspecified),
            _ => dateTime2.Date,
        };
    }

    /// <summary>
    /// Gets the object value from the source based on the configuration.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <returns>The object value.</returns>
    internal static object? GetObjectFromSource(
        object? source,
        DateTime2Configuration? configuration,
        IJsonSerializer jsonSerializer)
    {
        DateTime2Dto? intermediateValue = GetIntermediateFromSource(source, jsonSerializer);
        return GetObjectFromIntermediate(intermediateValue, configuration);
    }

    private static Type GetPropertyValueType(DateTime2Configuration? config) =>
        config?.Format switch
        {
            DateTime2Configuration.DateTimeFormat.DateOnly => typeof(DateOnly?),
            DateTime2Configuration.DateTimeFormat.TimeOnly => typeof(TimeOnly?),
            DateTime2Configuration.DateTimeFormat.DateTime when config.TimeZones?.Mode is not { } mode || mode == DateTime2Configuration.TimeZoneMode.None => typeof(DateTime?),
            _ => typeof(DateTimeOffset?),
        };

    /// <summary>
    ///     Model/DTO that represents the JSON that DateTime2 stores.
    /// </summary>
    internal class DateTime2Dto
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; init; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; init; }
    }
}
