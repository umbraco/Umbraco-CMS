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

        string? sourceString = source?.ToString();
        if (string.IsNullOrEmpty(sourceString) == false)
        {
            string[] rawValues = sourceString.Split(Constants.CharArrays.Comma);
            if (decimal.TryParse(rawValues[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal minimum))
            {
                if (isRange)
                {
                    if (rawValues.Length == 1)
                    {
                        // Configuration is probably changed from single to range, return range with same min/max
                        return new Range<decimal>
                        {
                            Minimum = minimum,
                            Maximum = minimum
                        };
                    }
                    else if (rawValues.Length == 2 && decimal.TryParse(rawValues[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal maximum))
                    {
                        return new Range<decimal>
                        {
                            Minimum = minimum,
                            Maximum = maximum
                        };
                    }
                }
                else
                {
                    // Return single value, regardless of whether it contains a range
                    return minimum;
                }
            }
        }

        // No value or parsing failed
        return isRange
            ? new Range<decimal>()
            : default(decimal);
    }

    private static bool IsRange(IPublishedPropertyType propertyType)
        => propertyType.DataType.ConfigurationAs<SliderConfiguration>()?.EnableRange == true;
}
