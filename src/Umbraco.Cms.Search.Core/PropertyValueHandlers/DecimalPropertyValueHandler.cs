using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes decimal property values as decimals.
/// </summary>
internal sealed class DecimalPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.Decimal
            or Cms.Core.Constants.PropertyEditors.Aliases.PlainDecimal;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
        => property.GetValue(culture, segment, published) is decimal decimalValue
            ? [new IndexField(property.Alias, new IndexValue { Decimals = [decimalValue] }, culture, segment)]
            : [];
}
