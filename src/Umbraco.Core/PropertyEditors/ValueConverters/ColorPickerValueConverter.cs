using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof(string))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class ColorPickerValueConverter : PropertyValueConverterBase
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
            return source == null ? string.Empty : source.ToString();
        }
        
    }
}
