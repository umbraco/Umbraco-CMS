using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class CheckboxListValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.CheckBoxListAlias);
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            var sourceString = (source ?? "").ToString();

            if (string.IsNullOrEmpty(sourceString))
                return Enumerable.Empty<string>();

            var values =
                sourceString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim());

            return values;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IEnumerable<string>);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType,
                                                        PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}
