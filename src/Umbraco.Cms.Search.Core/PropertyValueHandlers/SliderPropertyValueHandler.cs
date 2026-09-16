using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes slider property values as decimals (one value, or two for a range slider).
/// </summary>
internal sealed class SliderPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Slider;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var values = ParsePropertyValue(property, culture, segment, published);
        return values is not null
            ? [new IndexField(property.Alias, new IndexValue { Decimals = values }, culture, segment)]
            : [];
    }

    private static decimal[]? ParsePropertyValue(IProperty property, string? culture, string? segment, bool published)
    {
        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        var parts = value.Split(Cms.Core.Constants.CharArrays.Comma);
        var parsed = parts
            .Select(s => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var i) ? i : (decimal?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToArray();

        return parsed.Length == parts.Length && parsed.Length <= 2
            ? parsed
            : null;
    }
}
