using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The base DateTime property value converter.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public abstract class DateTimeValueConverterBase : PropertyValueConverterBase
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeValueConverterBase"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="logger">The logger.</param>
    protected DateTimeValueConverterBase(IJsonSerializer jsonSerializer, ILogger logger)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public abstract override bool IsConverter(IPublishedPropertyType propertyType);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        object? source,
        bool preview)
        => DateTimePropertyEditorHelper.TryParseToIntermediateValue(source, _jsonSerializer, _logger, out DateTimeDto? intermediateValue) ? intermediateValue : null;

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(
        IPublishedElement owner,
        IPublishedPropertyType propertyType,
        PropertyCacheLevel referenceCacheLevel,
        object? inter,
        bool preview) => inter is not DateTimeDto dateTime ? null : ConvertToObject(dateTime);

    /// <summary>
    /// Convert the intermediate representation to the final object.
    /// </summary>
    /// <param name="dateTimeDto">The intermediate representation.</param>
    /// <returns>The final object.</returns>
    protected abstract object ConvertToObject(DateTimeDto dateTimeDto);

    /// <summary>
    ///     Model/DTO that represents the JSON that DateTime2 stores.
    /// </summary>
    public class DateTimeDto
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; init; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; init; }
    }
}
