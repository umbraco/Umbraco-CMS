using System;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class TextStringValueConverter : PropertyValueConverterBase
    {
        private static readonly string[] PropertyTypeAliases =
        {
            Constants.PropertyEditors.TextboxAlias,
            Constants.PropertyEditors.TextboxMultipleAlias
        };

        public override bool IsConverter(PublishedPropertyType propertyType)
            => PropertyTypeAliases.Contains(propertyType.PropertyEditorAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (string);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Content;

        public override object ConvertSourceToInter(IPropertySet owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a string is: string
            // in the database a string is: string
            // default value is: null
            return source;
        }

        public override object ConvertInterToObject(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter ?? string.Empty;
        }

        public override object ConvertInterToXPath(IPropertySet owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter;
        }
    }
}
