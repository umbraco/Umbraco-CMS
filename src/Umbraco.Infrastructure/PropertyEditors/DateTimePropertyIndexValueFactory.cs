using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides base functionality for date time property editors index value factories that store their value as a JSON string with timezone information.
/// </summary>
internal abstract class DateTimePropertyIndexValueFactory : IPropertyIndexValueFactory
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<DateTimePropertyIndexValueFactory> _logger;

    protected DateTimePropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        ILogger<DateTimePropertyIndexValueFactory> logger)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<IndexValue> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var indexValue = new IndexValue
        {
            Culture = culture,
            FieldName = property.Alias,
            Values = [],
        };

        var sourceValue = property.GetValue(culture, segment, published);
        if (sourceValue is null
            || DateTimePropertyEditorHelper.TryParseToIntermediateValue(sourceValue, _jsonSerializer, _logger, out DateTimeValueConverterBase.DateTimeDto? dateTimeDto) is false
            || dateTimeDto is null)
        {
            return [indexValue];
        }

        var valueStr = MapDateToIndexValueFormat(dateTimeDto.Date);
        indexValue.Values = [valueStr];

        return [indexValue];
    }

    /// <summary>
    /// Maps the date to the appropriate string format for indexing.
    /// </summary>
    /// <param name="date">The date to map.</param>
    /// <returns>The formatted date string.</returns>
    protected abstract string MapDateToIndexValueFormat(DateTimeOffset date);
}
