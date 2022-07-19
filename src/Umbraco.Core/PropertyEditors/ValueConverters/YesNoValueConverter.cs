using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

[DefaultPropertyValueConverter]
public class YesNoValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.Boolean;

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(bool);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        // in xml a boolean is: string
        // in the database a boolean is: string "1" or "0" or empty
        // typically the converter does not need to handle anything else ("true"...)
        // however there are cases where the value passed to the converter could be a non-string object, e.g. int, bool
        if (source is string s)
        {
            if (s.Length == 0 || s == "0")
            {
                return false;
            }

            if (s == "1")
            {
                return true;
            }

            return bool.TryParse(s, out var result) && result;
        }

        if (source is int)
        {
            return (int)source == 1;
        }

        // this is required for correct true/false handling in nested content elements
        if (source is long)
        {
            return (long)source == 1;
        }

        if (source is bool)
        {
            return (bool)source;
        }

        // default value is: false
        return false;
    }

    // default ConvertSourceToObject just returns source ie a boolean value
    public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview) =>

        // source should come from ConvertSource and be a boolean already
        (bool?)inter ?? false ? "1" : "0";
}
