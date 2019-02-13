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
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DropDownListFlexible);
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if(source == null) return Array.Empty<string>();


            return JsonConvert.DeserializeObject<string[]>(source.ToString()) ?? Array.Empty<string>();
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
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

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return propertyType.DataType.ConfigurationAs<DropDownFlexibleConfiguration>().Multiple
               ? typeof(IEnumerable<string>)
               : typeof(string);
        }
    }
}
