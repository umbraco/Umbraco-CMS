using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Constants = Umbraco.Cms.Core.Constants;

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
    }
}
