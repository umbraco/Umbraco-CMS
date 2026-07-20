using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class MultiUrlPickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    public MultiUrlPickerPropertyValueHandler(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    public bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MultiUrlPicker;

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
