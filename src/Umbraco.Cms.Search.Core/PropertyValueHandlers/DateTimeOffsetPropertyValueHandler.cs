using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class DateTimeOffsetPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;

    public DateTimeOffsetPropertyValueHandler(IDateTimeOffsetConverter dateTimeOffsetConverter)
        => _dateTimeOffsetConverter = dateTimeOffsetConverter;

    public bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.DateTime
            or Cms.Core.Constants.PropertyEditors.Aliases.PlainDateTime;

    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
        => property.GetValue(culture, segment, published) is DateTime dateTime
            ? [new IndexField(property.Alias, new IndexValue { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(dateTime)] }, culture, segment)]
            : [];
}
