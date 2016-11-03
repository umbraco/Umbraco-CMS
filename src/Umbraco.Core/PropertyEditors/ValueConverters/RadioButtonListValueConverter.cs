using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class RadioButtonListValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.RadioButtonListAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var intAttempt = source.TryConvertTo<int>();
            if (intAttempt.Success)
                return intAttempt.Result;

            return null;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(int);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType,
                                                        PropertyCacheValue cacheValue)
        {
            return PropertyCacheLevel.Content;
        }
    }
}
