using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DateTime2PropertyIndexValueFactory : IDateTimeUnspecifiedPropertyIndexValueFactory,
    IDateTimeWithTimeZonePropertyIndexValueFactory,
    IDateOnlyPropertyIndexValueFactory,
    ITimeOnlyPropertyIndexValueFactory
{
    private readonly DateTime2ValueConverterBase _valueConverter;

    public DateTime2PropertyIndexValueFactory(
        DateTime2ValueConverterBase valueConverter) =>
        _valueConverter = valueConverter;

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

        var propertyValue = property.GetValue(culture, segment, published);
        if (propertyValue is null)
        {
            return [indexValue];
        }

        var value = GetValueFromSource(propertyValue);
        if (value is null)
        {
            return [indexValue];
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            // Index the DateTimeOffset as UTC, so it's easier to query.
            value = DateTime.SpecifyKind(dateTimeOffset.UtcDateTime, DateTimeKind.Unspecified);
        }

        indexValue.Values = [$"{value:O}"];

        return [indexValue];
    }

    /// <summary>
    /// Gets the value from the source using the value converter.
    /// </summary>
    /// <param name="source">The source value.</param>
    /// <returns>The converted value.</returns>
    protected object? GetValueFromSource(object? source)
    {
        DateTime2ValueConverterBase.DateTime2Dto? inter = _valueConverter.GetIntermediateFromSource(source);
        return inter is null ? null : _valueConverter.ConvertToObject(inter);
    }
}
