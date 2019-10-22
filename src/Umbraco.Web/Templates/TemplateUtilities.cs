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
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using File = System.IO.File;

namespace Umbraco.Web.Templates
{

    [Obsolete("This class is obsolete, all methods have been moved to other classes such as InternalLinkHelper, UrlResolver and MediaParser")]
    public static class TemplateUtilities
    {
        [Obsolete("Inject and use an instance of InternalLinkParser instead")]
        internal static string ParseInternalLinks(string text, bool preview, UmbracoContext umbracoContext)
        {
            using (umbracoContext.ForcedPreview(preview)) // force for url provider
            {
                text = ParseInternalLinks(text, umbracoContext.UrlProvider);
            }

            return text;
        }

        [Obsolete("Inject and use an instance of InternalLinkParser instead")]
        public static string ParseInternalLinks(string text, UrlProvider urlProvider)
            => Current.Factory.GetInstance<LocalLinkParser>().EnsureInternalLinks(text);

        [Obsolete("Inject and use an instance of UrlResolver")]
        public static string ResolveUrlsFromTextString(string text)
            => Current.Factory.GetInstance<UrlParser>().EnsureUrls(text);

        [Obsolete("Use StringExtensions.CleanForXss instead")]
        public static string CleanForXss(string text, params char[] ignoreFromClean)
            => text.CleanForXss(ignoreFromClean);

        [Obsolete("Use MediaParser.EnsureImageSources instead")]
        public static string ResolveMediaFromTextString(string text)
            => Current.Factory.GetInstance<ImageSourceParser>().EnsureImageSources(text);
        
        [Obsolete("Use MediaParser.RemoveImageSources instead")]
        internal static string RemoveMediaUrlsFromTextString(string text)
            => Current.Factory.GetInstance<ImageSourceParser>().RemoveImageSources(text);

        [Obsolete("Use MediaParser.RemoveImageSources instead")]
        internal static string FindAndPersistPastedTempImages(string html, Guid mediaParentFolder, int userId, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, ILogger logger)
            => Current.Factory.GetInstance<ImageSourceParser>().FindAndPersistPastedTempImages(html, mediaParentFolder, userId);
    }
}
