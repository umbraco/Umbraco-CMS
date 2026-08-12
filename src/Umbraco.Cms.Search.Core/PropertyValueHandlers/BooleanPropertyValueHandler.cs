using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes boolean property values as integers (0 or 1).
/// </summary>
internal sealed class BooleanPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Boolean;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var value = ParsePropertyValue(property, culture, segment, published);
        return value.HasValue
            ? [new IndexField(property.Alias, new IndexValue { Integers = [value.Value] }, culture, segment)]
            : [];
    }

    private static int? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var value = property.GetValue(culture, segment, published);
        return value switch
        {
            bool booleanValue => booleanValue ? 1 : 0,
            int integerValue => integerValue,
            _ => null
        };
    }
}
