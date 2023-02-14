using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// The slider property value converter.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.PropertyEditors.PropertyValueConverterBase" />
[DefaultPropertyValueConverter]
public class SliderValueConverter : PropertyValueConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SliderValueConverter" /> class.
    /// </summary>
    public SliderValueConverter()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SliderValueConverter" /> class.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    [Obsolete("The IDataTypeService is not used anymore. This constructor will be removed in a future version.")]
    public SliderValueConverter(IDataTypeService dataTypeService)
    { }

    /// <summary>
    /// Clears the data type configuration caches.
    /// </summary>
    [Obsolete("Caching of data type configuration is not done anymore. This method will be removed in a future version.")]
    public static void ClearCaches()
    { }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Slider);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => IsRange(propertyType) ? typeof(Range<decimal>) : typeof(decimal);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object? source, bool preview)
    {
        bool isRange = IsRange(propertyType);

        var sourceString = source?.ToString();

        return isRange
            ? HandleRange(sourceString)
            : HandleDecimal(sourceString);
    }

    private static Range<decimal> HandleRange(string? sourceString)
    {
        if (sourceString is null)
        {
            return new Range<decimal>();
        }

        string[] rangeRawValues = sourceString.Split(Constants.CharArrays.Comma);

        if (TryParseDecimal(rangeRawValues[0], out var minimum))
        {
            if (rangeRawValues.Length == 1)
            {
                // Configuration is probably changed from single to range, return range with same min/max
                return new Range<decimal>
                {
                    Minimum = minimum,
                    Maximum = minimum
                };
            }

            if (rangeRawValues.Length == 2 && TryParseDecimal(rangeRawValues[1], out var maximum))
            {
                return new Range<decimal>
                {
                    Minimum = minimum,
                    Maximum = maximum
                };
            }
        }

        return new Range<decimal>();
    }

    private static decimal HandleDecimal(string? sourceString)
    {
        if (string.IsNullOrEmpty(sourceString))
        {
            return default;
        }

        // This used to be a range slider, so we'll assign the minimum value as the new value
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
    /// Helper method for parsing a double consistently
    /// </summary>
    private static bool TryParseDecimal(string? representation, out decimal value)
        => decimal.TryParse(representation, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private static bool IsRange(IPublishedPropertyType propertyType)
        => propertyType.DataType.ConfigurationAs<SliderConfiguration>()?.EnableRange == true;
}
