using MarkdownSharp;
using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MarkdownEditorValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
            => Constants.PropertyEditors.MarkdownEditorAlias == propertyType.PropertyEditorAlias;

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (IHtmlString);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Facade;

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
            // convert markup to html for frontend rendering.
            // source should come from ConvertSource and be a string (or null) already
            var mark = new Markdown();
            return new HtmlString(inter == null ? string.Empty : mark.Transform((string)inter));
        }
    }
}
