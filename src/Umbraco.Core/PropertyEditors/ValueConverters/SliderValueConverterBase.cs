using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Holds what the slider value converters share.
/// </summary>
/// <remarks>
/// Both slider editors store their value as a string: a single decimal, or two separated by a comma. Each converter
/// reads the shape its own editor writes, so only the parsing of a single value is shared.
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
