using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes plain string property values (textbox, textarea) as full-text.
/// </summary>
internal sealed class PlainStringPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.TextBox
            or Cms.Core.Constants.PropertyEditors.Aliases.TextArea
            or Cms.Core.Constants.PropertyEditors.Aliases.PlainString;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
        => property.GetValue(culture, segment, published) is string stringValue
           && string.IsNullOrWhiteSpace(stringValue) is false
            ? [new IndexField(property.Alias, new IndexValue { Texts = [stringValue] }, culture, segment)]
            : [];
}
