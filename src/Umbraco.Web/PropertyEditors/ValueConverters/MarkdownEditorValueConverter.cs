using System;
using System.Web;
using HeyRed.MarkdownSharp;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Composing;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MarkdownEditorValueConverter : PropertyValueConverterBase
    {
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly HtmlUrlParser _urlParser;

        [Obsolete("Use ctor defining all dependencies instead")]
        public MarkdownEditorValueConverter()
            : this(Current.Factory.GetInstance<HtmlLocalLinkParser>(), Current.Factory.GetInstance<HtmlUrlParser>())
        {
        }

        public MarkdownEditorValueConverter(HtmlLocalLinkParser localLinkParser, HtmlUrlParser urlParser)
        {
            _localLinkParser = localLinkParser;
            _urlParser = urlParser;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => Constants.PropertyEditors.Aliases.MarkdownEditor == propertyType.EditorAlias;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IHtmlString);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensures string is parsed for {localLink} and URLs are resolved correctly
            sourceString = _localLinkParser.EnsureInternalLinks(sourceString, preview);
            sourceString = _urlParser.EnsureUrls(sourceString);

            return sourceString;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // convert markup to HTML for frontend rendering.
            // source should come from ConvertSource and be a string (or null) already
            var mark = new Markdown();
            return new HtmlString(inter == null ? string.Empty : mark.Transform((string)inter));
        }
    }
}
