using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes content picker property values as the picked content's key (Keyword).
/// </summary>
internal sealed class ContentPickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.ContentPicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        Guid? key = ParsePropertyValue(property, culture, segment, published);
        return key.HasValue
            ? [new IndexField(property.Alias, new IndexValue { Keywords = [key.Value.AsKeyword()] }, culture, segment)]
            : [];
    }

    private static Guid? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace()
            || UdiParser.TryParse(value, out Udi? udi) is false
            || udi is not GuidUdi guidUdi)
        {
            return null;
        }

        return guidUdi.Guid;
    }
}
