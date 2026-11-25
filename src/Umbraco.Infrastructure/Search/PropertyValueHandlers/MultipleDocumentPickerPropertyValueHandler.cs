using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Indexes multiple document picker property values as the picked documents' keys (Keywords).
/// </summary>
internal sealed class MultipleDocumentPickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MultipleDocumentPicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        IReadOnlyList<Guid> keys = EntityPickerValueParser.ParseKeys(
            property.GetValue(culture, segment, published),
            Umbraco.Cms.Core.Constants.UdiEntityType.Document);

        return keys.Count > 0
            ? [new IndexField(property.Alias, new IndexValue { Keywords = keys.Select(key => key.AsKeyword()).ToArray() }, culture, segment)]
            : [];
    }
}
