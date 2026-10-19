using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Indexes multiple textstring property values as text, one value per line.
/// </summary>
internal sealed class MultipleTextstringPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MultipleTextstring;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var values = ParsePropertyValue(property, culture, segment, published);
        return values?.Any() is true
            ? [new IndexField(property.Alias, new IndexValue { Texts = values }, culture, segment)]
            : [];
    }

    private static string[]? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var values = (property.GetValue(culture, segment, published) as string)?.Split("\n");
        return values;
    }
}
