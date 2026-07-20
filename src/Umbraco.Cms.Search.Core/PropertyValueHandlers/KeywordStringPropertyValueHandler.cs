using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class KeywordStringPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    public KeywordStringPropertyValueHandler(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    public bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.DropDownListFlexible or Cms.Core.Constants.PropertyEditors.Aliases.RadioButtonList or Cms.Core.Constants.PropertyEditors.Aliases.CheckBoxList;

    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace())
        {
            return [];
        }

        var keywords = value.DetectIsJson()
            ? _jsonSerializer.Deserialize<string[]>(value)
            : [value];
        return keywords?.Length > 0
            ? [new IndexField(property.Alias, new IndexValue { Keywords = keywords }, culture, segment)]
            : [];
    }
}
