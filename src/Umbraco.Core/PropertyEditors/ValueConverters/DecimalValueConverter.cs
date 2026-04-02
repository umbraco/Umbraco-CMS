using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for decimal properties.
/// </summary>
[DefaultPropertyValueConverter]
public class DecimalValueConverter : PropertyValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.Decimal.Equals(propertyType.EditorAlias);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(decimal);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => ParseDecimalValue(source);

    /// <summary>
    ///     Parses a source value to a decimal value.
    /// </summary>
    /// <param name="source">The source value to parse.</param>
    /// <returns>The parsed decimal value, or 0 if parsing fails.</returns>
    internal static decimal ParseDecimalValue(object? source)
    {
        if (source == null)
        {
            return 0M;
        }

        // is it already a decimal?
        if (source is decimal sourceDecimal)
        {
            return sourceDecimal;
        }

        // is it a double?
        if (source is double sourceDouble)
        {
            return Convert.ToDecimal(sourceDouble);
        }

        // is it an integer?
        if (source is int sourceInteger)
        {
            return Convert.ToDecimal(sourceInteger);
        }

        // is it a string?
        if (source is string sourceString)
        {
            return decimal.TryParse(sourceString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var d)
                ? d
                : 0M;
        }

        // couldn't convert the source value - default to zero
        return 0M;
    }
}
