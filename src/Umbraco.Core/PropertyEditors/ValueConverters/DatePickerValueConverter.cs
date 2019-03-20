﻿using System;
using System.Linq;
using System.Xml;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class DatePickerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.DateTime);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (DateTime);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return DateTime.MinValue;

            // in XML a DateTime is: string - format "yyyy-MM-ddTHH:mm:ss"
            // Actually, not always sometimes it is formatted in UTC style with 'Z' suffixed on the end but that is due to this bug:
            // http://issues.umbraco.org/issue/U4-4145, http://issues.umbraco.org/issue/U4-3894
            // We should just be using TryConvertTo instead.

            if (source is string sourceString)
            {
                var attempt = sourceString.TryConvertTo<DateTime>();
                return attempt.Success == false ? DateTime.MinValue : attempt.Result;
            }

            // in the database a DateTime is: DateTime
            // default value is: DateTime.MinValue
            return source is DateTime ? source : DateTime.MinValue;
        }

        // default ConvertSourceToObject just returns source ie a DateTime value

        public override object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a DateTime already
            return XmlConvert.ToString((DateTime) inter, XmlDateTimeSerializationMode.Unspecified);
        }
    }
}
