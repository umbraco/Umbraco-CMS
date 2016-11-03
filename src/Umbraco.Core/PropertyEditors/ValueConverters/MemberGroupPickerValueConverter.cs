using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class MemberGroupPickerValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberGroupPickerAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source == null ? string.Empty : source.ToString();
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(string);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType,
                                                        PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}
