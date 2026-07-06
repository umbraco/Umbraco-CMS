using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class LabelPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    public LabelPropertyValueHandler(IDateTimeOffsetConverter dateTimeOffsetConverter, IDataTypeConfigurationCache dataTypeConfigurationCache)
    {
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
    }

    public bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Label;

    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var value = property.GetValue(culture, segment, published);
        if (value is null || string.Empty.Equals(value))
        {
            return [];
        }

        LabelConfiguration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<LabelConfiguration>(property.PropertyType.DataTypeKey);
        if (configuration is null)
        {
            return [];
        }

        IndexValue? indexValue = configuration.ValueType switch
        {
            ValueTypes.Integer when value is int integerValue
                => new IndexValue { Integers = [integerValue] },
            ValueTypes.Decimal when value is decimal decimalValue
                => new IndexValue { Decimals = [decimalValue] },
            ValueTypes.Date or ValueTypes.DateTime when value is DateTime dateTimeValue
                => new IndexValue { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(dateTimeValue)] },
            ValueTypes.String when value is string stringValue
                => new IndexValue { Texts = [stringValue] },
            ValueTypes.Bigint when value is string stringValue && int.TryParse(stringValue, out var integerValue)
                => new IndexValue { Integers = [integerValue] },
            _ => null
        };

        return indexValue is not null
            ? [new IndexField(property.Alias, indexValue, culture, segment)]
            : [];
    }
}
