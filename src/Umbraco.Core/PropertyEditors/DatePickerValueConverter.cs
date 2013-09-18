using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors
{
    [PropertyValueType(typeof(DateTime))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    internal class DatePickerValueConverter : PropertyValueConverterBase
	{
	    private static readonly Guid[] DataTypeGuids = new[]
	        {
	            Guid.Parse(Constants.PropertyEditors.DateTime),
	            Guid.Parse(Constants.PropertyEditors.Date)
	        };

        public override bool IsDataToSourceConverter(PublishedPropertyType propertyType)
        {
            return DataTypeGuids.Contains(propertyType.PropertyEditorGuid);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return DateTime.MinValue;

            // in XML a DateTime is: string - format "yyyy-MM-ddTHH:mm:ss"
            var sourceString = source as string;
            if (sourceString != null)
            {
                DateTime value;
                return DateTime.TryParseExact(sourceString, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out value) 
                    ? value 
                    : DateTime.MinValue;
            }

            // in the database a DateTime is: DateTime
            // default value is: DateTime.MinValue
            return (source is DateTime) 
                ? source 
                : DateTime.MinValue;
        }

        public override bool IsSourceToObjectConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        // default ConvertSourceToObject just returns source ie a DateTime value

        public override bool IsSourceToXPathConverter(PublishedPropertyType propertyType)
        {
            return IsDataToSourceConverter(propertyType);
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a DateTime already
            return XmlConvert.ToString((DateTime) source, "yyyy-MM-ddTHH:mm:ss");
        }
    }
}