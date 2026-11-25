using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Indexes multiple document picker property values as the picked documents' keys (Keywords).
/// </summary>
internal sealed class MultipleDocumentPickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleDocumentPickerPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the picked keys.</param>
    public MultipleDocumentPickerPropertyValueHandler(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MultipleDocumentPicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        IReadOnlyList<Guid> keys = EntityPickerValueParser.ParseKeys(
            property.GetValue(culture, segment, published),
            Umbraco.Cms.Core.Constants.UdiEntityType.Document,
            _jsonSerializer);

        return keys.Count > 0
            ? [new IndexField(property.Alias, new IndexValue { Keywords = keys.Select(key => key.AsKeyword()).ToArray() }, culture, segment)]
            : [];
    }
}
