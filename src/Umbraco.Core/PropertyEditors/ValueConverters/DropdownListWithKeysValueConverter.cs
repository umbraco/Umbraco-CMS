using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class DropdownListWithKeysValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.DropdownlistPublishingKeysAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (int);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Content;

        public override object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            var intAttempt = source.TryConvertTo<int>();
            if (intAttempt.Success)
                return intAttempt.Result;

            return null;
        }
    }
}
