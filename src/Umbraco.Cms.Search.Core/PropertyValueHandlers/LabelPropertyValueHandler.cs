using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes label property values as the underlying value type configured for the label (integer, decimal, date or string).
/// </summary>
internal sealed class LabelPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="dateTimeOffsetConverter">The converter used to normalize date-typed label values to <see cref="DateTimeOffset"/>.</param>
    /// <param name="dataTypeConfigurationCache">The cache used to resolve the label's configured underlying value type.</param>
    public LabelPropertyValueHandler(IDateTimeOffsetConverter dateTimeOffsetConverter, IDataTypeConfigurationCache dataTypeConfigurationCache)
    {
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _dataTypeConfigurationCache = dataTypeConfigurationCache;
    }

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Label;

    /// <inheritdoc />
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
