using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes exact-match string property values (dropdowns, radio buttons, checkbox lists) as keywords.
/// </summary>
internal sealed class KeywordStringPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeywordStringPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize JSON-stored multi-value selections.</param>
    public KeywordStringPropertyValueHandler(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.DropDownListFlexible
            or Cms.Core.Constants.PropertyEditors.Aliases.RadioButtonList
            or Cms.Core.Constants.PropertyEditors.Aliases.CheckBoxList;

    /// <inheritdoc />
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
