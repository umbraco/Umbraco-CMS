using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The base DateTime2 property value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public abstract class DateTime2ValueConverterBase : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTime2ValueConverterBase"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected DateTime2ValueConverterBase(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public abstract override bool IsConverter(IPublishedPropertyType propertyType);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => GetPropertyValueType();

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        object? source,
        bool preview)
        => GetIntermediateFromSource(source);

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel,
        object? inter,
        bool preview) =>
        inter is not DateTime2Dto dateTime2
            ? null
            : ConvertToObject(dateTime2);

    /// <summary>
    /// Convert the intermediate representation to the final object.
    /// </summary>
    /// <param name="dateTimeDto">The intermediate representation.</param>
    /// <returns>The final object.</returns>
    internal abstract object ConvertToObject(DateTime2Dto dateTimeDto);

    /// <summary>
    /// Converts the source value to an intermediate representation.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <returns>The intermediate representation.</returns>
    internal DateTime2Dto? GetIntermediateFromSource(object? source) =>
        source switch
        {
            // This DateTime check is for compatibility with the "deprecated" `Umbraco.DateTime`.
            // Once that is removed, this can be removed too.
            DateTime dateTime => new DateTime2Dto { Date = new DateTimeOffset(dateTime, TimeSpan.Zero) },
            string sourceStr => _jsonSerializer.TryDeserialize(sourceStr, out DateTime2Dto? dateTime2) ? dateTime2 : null,
            _ => null,
        };

    /// <summary>
    ///    Gets the property value type for this value converter.
    /// </summary>
    /// <returns>The property value type.</returns>
    protected abstract Type GetPropertyValueType();

    /// <summary>
    ///     Model/DTO that represents the JSON that DateTime2 stores.
    /// </summary>
    protected internal class DateTime2Dto
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; init; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; init; }
    }
}
