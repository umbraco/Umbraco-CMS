using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class FlexibleDropdownPropertyValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DropDownListFlexible);
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if(source == null) return Array.Empty<string>();


            return JsonConvert.DeserializeObject<string[]>(source.ToString()) ?? Array.Empty<string>();
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null)
                return null;

            var multiple = propertyType.DataType.ConfigurationAs<DropDownFlexibleConfiguration>().Multiple;
            var selectedValues = (string[]) inter;
            if (selectedValues.Length > 0)
            {
                return multiple
                    ? (object) selectedValues
                    : selectedValues[0];
            }

            return multiple
                ? inter
                : string.Empty;
        }

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            return propertyType.DataType.ConfigurationAs<DropDownFlexibleConfiguration>().Multiple
               ? typeof(IEnumerable<string>)
               : typeof(string);
        }
        
        //an empty dropdown will also store a value as a string in the format '[]' or '[null]'
        public override bool HasValue(IPublishedProperty property, string culture, string segment)
        {
            var value = property.GetSourceValue(culture, segment);
            return value != null && (!(value is string) || (string.IsNullOrWhiteSpace((string)value) == false && !string.Equals((string)value, "[]") && !string.Equals((string)value, "[null]", StringComparison.CurrentCultureIgnoreCase)));
        }
        public override bool? IsValue(object value, PropertyValueLevel level)
        {
            switch (level)
            {
                case PropertyValueLevel.Source:
                    return value != null && (!(value is string) || (string.IsNullOrWhiteSpace((string)value) == false && !string.Equals((string)value, "[]") && !string.Equals((string)value, "[null]", StringComparison.CurrentCultureIgnoreCase)));
                default:
                    throw new NotSupportedException($"Invalid level: {level}.");
            }
        }
    }
}
