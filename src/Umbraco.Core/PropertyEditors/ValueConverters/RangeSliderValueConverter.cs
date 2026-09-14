using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The value converter for the slider property editor holding a range of two values.
/// </summary>
/// <seealso cref="SliderValueConverterBase" />
[DefaultPropertyValueConverter]
public class RangeSliderValueConverter : SliderValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.RangeSlider);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(Range<decimal>);

    /// <inheritdoc />
    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => ReadRange(inter?.ToString());

    /// <summary>
    /// Reads a range from the stored value.
    /// </summary>
    /// <param name="sourceString">The stored value.</param>
    /// <returns>The range the stored value holds, or an empty range when it holds none.</returns>
    private static Range<decimal> ReadRange(string? sourceString)
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
}
