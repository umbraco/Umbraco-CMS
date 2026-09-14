using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Reads the value the slider editors store.
/// </summary>
/// <remarks>
/// Both slider editors store their value as a string: a single decimal, or two separated by a comma. The parsing is
/// deliberately tolerant of the other editor's shape, as a data type that held the other shape before the two were
/// separated still has values in it.
/// </remarks>
public abstract class SliderValueConverterBase : PropertyValueConverterBase
{
    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <summary>
    /// Helper method for parsing a decimal consistently.
    /// </summary>
    /// <param name="representation">The value to parse.</param>
    /// <param name="value">The parsed value, or the default when parsing failed.</param>
    /// <returns><c>true</c> if the value could be parsed; otherwise, <c>false</c>.</returns>
    protected static bool TryParseDecimal(string? representation, out decimal value)
        => decimal.TryParse(representation, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
}
