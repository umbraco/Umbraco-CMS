using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class DateTimeOffsetPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly IJsonSerializer _jsonSerializer;

    public DateTimeOffsetPropertyValueHandler(IDateTimeOffsetConverter dateTimeOffsetConverter, IJsonSerializer jsonSerializer)
    {
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _jsonSerializer = jsonSerializer;
    }

    public bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.DateTime
            or Cms.Core.Constants.PropertyEditors.Aliases.PlainDateTime
            or Cms.Core.Constants.PropertyEditors.Aliases.DateOnly
            or Cms.Core.Constants.PropertyEditors.Aliases.TimeOnly
            or Cms.Core.Constants.PropertyEditors.Aliases.DateTimeUnspecified
            or Cms.Core.Constants.PropertyEditors.Aliases.DateTimeWithTimeZone;

    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        object? value = property.GetValue(culture, segment, published);

        DateTimeOffset? dateTimeOffset = value switch
        {
            DateTime dateTime => _dateTimeOffsetConverter.ToDateTimeOffset(dateTime),
            string json when _jsonSerializer.TryDeserialize(json, out DateTimeValueConverterBase.DateTimeDto? dto) => dto.Date.ToUniversalTime(),
            _ => null,
        };

        return dateTimeOffset is not null
            ? [new IndexField(property.Alias, new IndexValue { DateTimeOffsets = [dateTimeOffset.Value] }, culture, segment)]
            : [];
    }
}
