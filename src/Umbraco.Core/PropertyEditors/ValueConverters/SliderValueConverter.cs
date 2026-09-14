using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The value converter for the slider property editor holding a single value.
/// </summary>
/// <seealso cref="SliderValueConverterBase" />
[DefaultPropertyValueConverter]
public class SliderValueConverter : SliderValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Slider);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(decimal);

    /// <inheritdoc />
    public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
        => ReadDecimal(source?.ToString());

    /// <summary>
    /// Reads a single value from the stored value.
    /// </summary>
    /// <param name="sourceString">The stored value.</param>
    /// <returns>The value the stored value holds, or the default when it holds none.</returns>
    private static decimal ReadDecimal(string? sourceString)
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
}
