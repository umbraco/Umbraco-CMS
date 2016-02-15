using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [PropertyValueType(typeof(string))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Request)]
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

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensures string is parsed for {localLink} and urls are resolved correctly
            sourceString = TemplateUtilities.ParseInternalLinks(sourceString, preview);
            sourceString = TemplateUtilities.ResolveUrlsFromTextString(sourceString);

            return sourceString;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return source ?? string.Empty;
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return source;
        }
    }
}
