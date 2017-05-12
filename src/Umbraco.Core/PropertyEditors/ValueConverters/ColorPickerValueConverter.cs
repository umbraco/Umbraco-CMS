using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class ColorPickerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
			return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ColorPickerAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(string);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Content;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            // make sure it's a string
            return source?.ToString() ?? string.Empty;
        }
    }
}
