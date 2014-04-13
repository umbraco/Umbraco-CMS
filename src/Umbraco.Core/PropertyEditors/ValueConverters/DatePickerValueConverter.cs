using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [PropertyValueType(typeof(DateTime))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class DatePickerValueConverter : PropertyValueConverterBase
	{
	    private static readonly string[] PropertyEditorAliases =
	    {
	        Constants.PropertyEditors.DateTimeAlias,
	        Constants.PropertyEditors.DateAlias
	    };

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return PropertyEditorAliases.Contains(propertyType.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return DateTime.MinValue;

            // in XML a DateTime is: string - format "yyyy-MM-ddTHH:mm:ss"
            // Actually, not always sometimes it is formatted in UTC style with 'Z' suffixed on the end but that is due to this bug:
            // http://issues.umbraco.org/issue/U4-4145, http://issues.umbraco.org/issue/U4-3894
            // We should just be using TryConvertTo instead.
            var sourceString = source as string;
            if (sourceString != null)
            {
                var attempt = sourceString.TryConvertTo<DateTime>();
                return attempt.Success == false ? DateTime.MinValue : attempt.Result;
            }

            // in the database a DateTime is: DateTime
            // default value is: DateTime.MinValue
            return (source is DateTime) 
                ? source 
                : DateTime.MinValue;
        }

        // default ConvertSourceToObject just returns source ie a DateTime value

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a DateTime already
            return XmlConvert.ToString((DateTime)source, XmlDateTimeSerializationMode.Unspecified);
        }
    }
}