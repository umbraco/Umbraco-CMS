using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Indexes content picker property values as the picked content's key (Keyword).
/// </summary>
internal sealed class ContentPickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPickerPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to read the picked key.</param>
    public ContentPickerPropertyValueHandler(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.ContentPicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        // A value holding several documents indexes the first, as this editor holds one.
        IReadOnlyList<Guid> keys = EntityPickerValueParser.ParseKeys(
            property.GetValue(culture, segment, published),
            Umbraco.Cms.Core.Constants.UdiEntityType.Document,
            _jsonSerializer);

        return keys.Count > 0
            ? [new IndexField(property.Alias, new IndexValue { Keywords = [keys[0].AsKeyword()] }, culture, segment)]
            : [];
    }
}
