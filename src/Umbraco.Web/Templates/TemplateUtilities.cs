using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using File = System.IO.File;

namespace Umbraco.Web.Templates
{

    [Obsolete("This class is obsolete, all methods have been moved to other classes: " + nameof(HtmlLocalLinkParser) + ", " + nameof(HtmlUrlParser) + " and " + nameof(HtmlImageSourceParser))]
    public static class TemplateUtilities
    {
        [Obsolete("Inject and use an instance of " + nameof(HtmlLocalLinkParser) + " instead")]
        internal static string ParseInternalLinks(string text, bool preview, UmbracoContext umbracoContext)
        {
            using (umbracoContext.ForcedPreview(preview)) // force for URL provider
            {
                text = ParseInternalLinks(text, umbracoContext.UrlProvider);
            }

            return text;
        }

        [Obsolete("Inject and use an instance of " + nameof(HtmlLocalLinkParser) + " instead")]
        public static string ParseInternalLinks(string text, UrlProvider urlProvider)
            => Current.Factory.GetInstance<HtmlLocalLinkParser>().EnsureInternalLinks(text);

        [Obsolete("Inject and use an instance of " + nameof(HtmlUrlParser))]
        public static string ResolveUrlsFromTextString(string text)
            => Current.Factory.GetInstance<HtmlUrlParser>().EnsureUrls(text);

        [Obsolete("Use " + nameof(StringExtensions) + "." + nameof(StringExtensions.CleanForXss) + " instead")]
        public static string CleanForXss(string text, params char[] ignoreFromClean)
            => text.CleanForXss(ignoreFromClean);

        [Obsolete("Use " + nameof(HtmlImageSourceParser) + "." + nameof(HtmlImageSourceParser.EnsureImageSources) + " instead")]
        public static string ResolveMediaFromTextString(string text)
            => Current.Factory.GetInstance<HtmlImageSourceParser>().EnsureImageSources(text);
        
        [Obsolete("Use " + nameof(HtmlImageSourceParser) + "." + nameof(HtmlImageSourceParser.RemoveImageSources) + " instead")]
        internal static string RemoveMediaUrlsFromTextString(string text)
            => Current.Factory.GetInstance<HtmlImageSourceParser>().RemoveImageSources(text);

        [Obsolete("Use " + nameof(HtmlImageSourceParser) + "." + nameof(RichTextEditorPastedImages.FindAndPersistPastedTempImages) + " instead")]
        internal static string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, ILogger logger)
            => Current.Factory.GetInstance<RichTextEditorPastedImages>().FindAndPersistPastedTempImages(html, mediaParentFolder, userId, Current.Factory.GetInstance<IImageUrlGenerator>());
    }
}
