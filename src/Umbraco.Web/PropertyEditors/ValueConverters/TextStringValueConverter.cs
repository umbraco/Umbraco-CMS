﻿using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter(typeof(Core.PropertyEditors.ValueConverters.TextStringValueConverter))]
    public class TextStringValueConverter : PropertyValueConverterBase
    {
        private readonly HtmlLocalLinkParser _linkParser;
        private readonly HtmlUrlParser _urlParser;

        public TextStringValueConverter(HtmlLocalLinkParser linkParser, HtmlUrlParser urlParser)
        {
            _linkParser = linkParser;
            _urlParser = urlParser;
        }

        private static readonly string[] PropertyTypeAliases =
        {
            Constants.PropertyEditors.Aliases.TextBox,
            Constants.PropertyEditors.Aliases.TextArea
        };
        
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => PropertyTypeAliases.Contains(propertyType.EditorAlias);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (string);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensures string is parsed for {localLink} and urls are resolved correctly
            sourceString = _linkParser.EnsureInternalLinks(sourceString, preview);
            sourceString = _urlParser.EnsureUrls(sourceString);

            return sourceString;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter ?? string.Empty;
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter;
        }
    }
}
