using System.Globalization;
using Umbraco.Cms.Core.Models;
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
    /// Reads a range from the stored value.
    /// </summary>
    protected static Range<decimal> ReadRange(string? sourceString)
    {
        if (sourceString is null)
        {
            return new Range<decimal>();
        }

        var rangeRawValues = sourceString.Split(Constants.CharArrays.Comma);

        if (TryParseDecimal(rangeRawValues[0], out var minimum))
        {
            if (rangeRawValues.Length == 1)
            {
                // The value was stored by the single value slider, so both ends of the range are that value.
                return new Range<decimal>
                {
                    Minimum = minimum,
                    Maximum = minimum,
                };
            }

            if (rangeRawValues.Length == 2 && TryParseDecimal(rangeRawValues[1], out var maximum))
            {
                return new Range<decimal>
                {
                    Minimum = minimum,
                    Maximum = maximum,
                };
            }
        }

        return new Range<decimal>();
    }

    /// <summary>
    /// Reads a single value from the stored value.
    /// </summary>
    protected static decimal ReadDecimal(string? sourceString)
    {
        if (string.IsNullOrEmpty(sourceString))
        {
            return default;
        }

        // The value was stored by the range slider, so the lower end of the range is the value.
        if (sourceString.Contains(','))
        {
            var minimumValueRepresentation = sourceString.Split(Constants.CharArrays.Comma)[0];

            if (TryParseDecimal(minimumValueRepresentation, out var minimum))
            {
                return minimum;
            }
        }
        else if (TryParseDecimal(sourceString, out var value))
        {
            return value;
        }

        return default;
    }

    /// <summary>
    /// Helper method for parsing a decimal consistently.
    /// </summary>
    protected static bool TryParseDecimal(string? representation, out decimal value)
        => decimal.TryParse(representation, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
}
