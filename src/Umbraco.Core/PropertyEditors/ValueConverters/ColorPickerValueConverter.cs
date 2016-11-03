using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class ColorPickerValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
	        if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
	        {
				return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ColorPickerAlias);
			}
	        return false;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            // make sure it's a string
            return source.ToString();
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(string);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}
