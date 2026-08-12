using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes multi URL picker property values as the picked links' display names (text).
/// </summary>
internal sealed class MultiUrlPickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the picked links.</param>
    public MultiUrlPickerPropertyValueHandler(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MultiUrlPicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var texts = ParsePropertyValue(property, culture, segment, published);
        return texts is not null
            ? [new IndexField(property.Alias, new IndexValue { Texts = texts }, culture, segment)]
            : [];
    }

    private string[]? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            MultiUrlPickerValueEditor.LinkDto[]? linkDtos = _jsonSerializer.Deserialize<MultiUrlPickerValueEditor.LinkDto[]>(value);
            return linkDtos?.Select(linkDto => linkDto.Name).WhereNotNull().ToArray();
        }
        catch
        {
            // silently fail - this is an invalid property value, expect it to be reported elsewhere
            return null;
        }
    }
}
