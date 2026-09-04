using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

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
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.MultipleDocumentPicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace())
        {
            return [];
        }

        Guid[]? keys = _jsonSerializer.Deserialize<Guid[]>(value);
        if (keys is null || keys.Length == 0)
        {
            return [];
        }

        return [new IndexField(property.Alias, new IndexValue { Keywords = keys.Select(key => key.AsKeyword()).ToArray() }, culture, segment)];
    }
}
