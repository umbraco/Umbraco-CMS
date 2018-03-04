using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof(int))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class IntegerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.IntegerAlias.Equals(propertyType.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
                return default(int);

            // in the database an integer is an integer
            if (source is int)
                return source;

            if (source is long)
                return Convert.ToInt32(source);

            // in XML an integer is a string
            int i;
            if (source is string && int.TryParse((string)source, out i))
                return i;

            // default value is zero
            return default(int);
        }
    }
}
