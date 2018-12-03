using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class EmailAddressValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.EmailAddress);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (string);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            return source?.ToString() ?? string.Empty;
        }
    }
}
