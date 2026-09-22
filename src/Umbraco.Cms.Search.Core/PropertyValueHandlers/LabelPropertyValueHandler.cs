using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Aliases = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes label property values as the type of value their editor holds (integer, decimal, date or string).
/// </summary>
internal sealed class LabelPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="dateTimeOffsetConverter">The converter used to normalize date-typed label values to <see cref="DateTimeOffset"/>.</param>
    public LabelPropertyValueHandler(IDateTimeOffsetConverter dateTimeOffsetConverter)
        => _dateTimeOffsetConverter = dateTimeOffsetConverter;

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is
            Aliases.Label
            or Aliases.LabelText
            or Aliases.LabelInteger
            or Aliases.LabelBigInt
            or Aliases.LabelDecimal
            or Aliases.LabelDateTime
            or Aliases.LabelTime;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var value = property.GetValue(culture, segment, published);
        if (value is null || string.Empty.Equals(value))
        {
            return [];
        }

        IndexValue? indexValue = property.PropertyType.PropertyEditorAlias switch
        {
            Aliases.LabelInteger when value is int integerValue
                => new IndexValue { Integers = [integerValue] },
            Aliases.LabelDecimal when value is decimal decimalValue
                => new IndexValue { Decimals = [decimalValue] },
            Aliases.LabelDateTime when value is DateTime dateTimeValue
                => new IndexValue { DateTimeOffsets = [_dateTimeOffsetConverter.ToDateTimeOffset(dateTimeValue)] },
            Aliases.Label or Aliases.LabelText when value is string stringValue
                => new IndexValue { Texts = [stringValue] },
            Aliases.LabelBigInt when value is string stringValue && int.TryParse(stringValue, out var integerValue)
                => new IndexValue { Integers = [integerValue] },
            _ => null,
        };

        return indexValue is not null
            ? [new IndexField(property.Alias, indexValue, culture, segment)]
            : [];
    }
}
