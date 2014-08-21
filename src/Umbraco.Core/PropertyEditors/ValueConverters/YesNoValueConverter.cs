using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [PropertyValueType(typeof(bool))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class YesNoValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias == Constants.PropertyEditors.TrueFalseAlias;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a boolean is: string
            // in the database a boolean is: string "1" or "0" or empty
            // typically the converter does not need to handle anything else ("true"...)
            // however there are cases where the value passed to the converter could be a non-string object, e.g. int, bool

            if (source is string)
            {
                var str = (string)source;

                if (str == null || str.Length == 0 || str == "0")
                    return false;

                if (str == "1")
                    return true;

                bool result;
                if (bool.TryParse(str, out result))
                    return result;

                return false;
            }

            if (source is int)
                return (int)source == 1;

            if (source is bool)
                return (bool)source;

            // default value is: false
            return false;
        }

        // default ConvertSourceToObject just returns source ie a boolean value

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a boolean already
            return (bool)source ? "1" : "0";
        }
    }
}
