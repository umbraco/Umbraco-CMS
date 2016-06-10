using System;
using System.Globalization;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class DecimalValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.DecimalAlias.Equals(propertyType.PropertyEditorAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof (decimal);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Content;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return 0M;

            // in XML a decimal is a string
            var sourceString = source as string;
            if (sourceString != null)
            {
                decimal d;
                return (decimal.TryParse(sourceString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,  out d)) ? d : 0M;
            }

            // in the database an a decimal is an a decimal 
            // default value is zero
            return (source is decimal) ? source : 0M;
        }
    }
}
