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
            // the converter does not need to handle anything else ("true"...)

            // default value is: false
            var sourceString = source as string;
            if (sourceString == null) return false;
            return sourceString == "1";
        }

        // default ConvertSourceToObject just returns source ie a boolean value

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a boolean already
            return (bool) source ? "1" : "0";
        }
    }
}
