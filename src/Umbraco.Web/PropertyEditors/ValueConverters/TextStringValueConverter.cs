using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    public class TextStringValueConverter : PropertyValueConverterBase
    {
        private readonly static string[] PropertyTypeAliases =
        {
            Constants.PropertyEditors.TextboxAlias,
            Constants.PropertyEditors.TextboxMultipleAlias
        };

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return PropertyTypeAliases.Contains(propertyType.PropertyEditorAlias);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof (string);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Facade;
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensures string is parsed for {localLink} and urls are resolved correctly
            sourceString = TemplateUtilities.ParseInternalLinks(sourceString, preview);
            sourceString = TemplateUtilities.ResolveUrlsFromTextString(sourceString);

            return sourceString;
        }

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter ?? string.Empty;
        }

        public override object ConvertInterToXPath(PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter;
        }
    }
}
