using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Indexes integer property values as integers.
/// </summary>
internal sealed class IntegerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.Integer
            or Cms.Core.Constants.PropertyEditors.Aliases.PlainInteger;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
        => property.GetValue(culture, segment, published) is int integerValue
            ? [new IndexField(property.Alias, new IndexValue { Integers = [integerValue] }, culture, segment)]
            : [];
}
