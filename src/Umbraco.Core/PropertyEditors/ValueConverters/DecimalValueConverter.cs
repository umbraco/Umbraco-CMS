using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class DecimalValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.Decimal.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(decimal);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return 0M;
        }

        // is it already a decimal?
        if (source is decimal)
        {
            return source;
        }

        // is it a double?
        if (source is double sourceDouble)
        {
            return Convert.ToDecimal(sourceDouble);
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
