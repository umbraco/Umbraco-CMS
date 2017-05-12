using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class CheckboxListValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.CheckBoxListAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IEnumerable<string>);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Content;
        }

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            var sourceString = (source ?? string.Empty).ToString();

            if (string.IsNullOrEmpty(sourceString))
                return Enumerable.Empty<string>();

            var values =
                sourceString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim());

            return values;
        }        
    }
}
