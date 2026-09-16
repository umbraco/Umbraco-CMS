using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes date/time property values (date, time, date-only, time-only and variants) as dates.
/// </summary>
internal sealed class DateTimeOffsetPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeOffsetPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="dateTimeOffsetConverter">The converter used to normalize <see cref="DateTime"/> property values to <see cref="DateTimeOffset"/>.</param>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize JSON-stored date/time values.</param>
    public DateTimeOffsetPropertyValueHandler(IDateTimeOffsetConverter dateTimeOffsetConverter, IJsonSerializer jsonSerializer)
    {
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.DateTime
            or Cms.Core.Constants.PropertyEditors.Aliases.PlainDateTime
            or Cms.Core.Constants.PropertyEditors.Aliases.DateOnly
            or Cms.Core.Constants.PropertyEditors.Aliases.TimeOnly
            or Cms.Core.Constants.PropertyEditors.Aliases.DateTimeUnspecified
            or Cms.Core.Constants.PropertyEditors.Aliases.DateTimeWithTimeZone;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        DateTimeOffset? dateTimeOffset = ParsePropertyValue(property, culture, segment, published);

        return dateTimeOffset is not null
            ? [new IndexField(property.Alias, new IndexValue { DateTimeOffsets = [dateTimeOffset.Value] }, culture, segment)]
            : [];
    }

    private DateTimeOffset? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        object? value = property.GetValue(culture, segment, published);

        try
        {
            return value switch
            {
                DateTime dateTime => _dateTimeOffsetConverter.ToDateTimeOffset(dateTime),
                string json when _jsonSerializer.TryDeserialize(json, out DateTimeValueConverterBase.DateTimeDto? dto) => dto.Date.ToUniversalTime(),
                _ => null,
            };
        }
        catch
        {
            // silently fail - this is an invalid property value, expect it to be reported elsewhere
            return null;
        }
    }
}
