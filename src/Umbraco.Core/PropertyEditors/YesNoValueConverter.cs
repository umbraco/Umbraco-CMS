using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    [PropertyValueType(typeof(bool))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    class YesNoValueConverter : IPropertyValueConverter
    {
        public bool IsDataToSourceConverter(PublishedPropertyType propertyType)
        {
            return Guid.Parse(Constants.PropertyEditors.TrueFalse).Equals(propertyType.EditorGuid);
        }

        public object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a boolean is: string
            // in the database a boolean is: string "1" or "0" or empty
            // the converter does not need to handle anything else ("true"...)

            // default value is: false
            var sourceString = source as string;
            if (sourceString == null) return false;
            return sourceString == "1";
        }

        public bool IsSourceToObjectConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a boolean already
            return source;
        }

        public bool IsSourceToXPathConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a boolean already
            return (bool) source ? "1" : "0";
        }
    }
}
