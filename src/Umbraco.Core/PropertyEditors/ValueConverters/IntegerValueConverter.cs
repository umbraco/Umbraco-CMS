using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class IntegerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => Constants.PropertyEditors.Aliases.Integer.Equals(propertyType.EditorAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (int);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return 0;

            // in XML an integer is a string
            var sourceString = source as string;
            if (sourceString != null)
            {
                int i;
                return (int.TryParse(sourceString, out i)) ? i : 0;
            }

            // in json an integer comes back as Int64
            // ignore overflows ;(
            if (source is long)
                return Convert.ToInt32(source);

            // in the database an integer is an integer
            // default value is zero
            return (source is int) ? source : 0;
        }
    }
}
