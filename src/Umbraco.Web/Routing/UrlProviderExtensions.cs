using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Routing
{
    internal static class UrlProviderExtensions
    {
        /// <summary>
        /// Gets the URLs of the content item.
        /// </summary>
        /// <remarks>
        /// <para>Use when displaying URLs. If errors occur when generating the URLs, they will show in the list.</para>
        /// <para>Contains all the URLs that we can figure out (based upon domains, etc).</para>
        /// </remarks>
        public static IEnumerable<UrlInfo> GetContentUrls(this IContent content,
            IPublishedRouter publishedRouter,
            UmbracoContext umbracoContext,
            ILocalizationService localizationService,
            ILocalizedTextService textService,
            IContentService contentService,
            ILogger logger)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (publishedRouter == null) throw new ArgumentNullException(nameof(publishedRouter));
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            if (localizationService == null) throw new ArgumentNullException(nameof(localizationService));
            if (textService == null) throw new ArgumentNullException(nameof(textService));
            if (contentService == null) throw new ArgumentNullException(nameof(contentService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            if (content.Published == false)
            {
                yield return UrlInfo.Message(textService.Localize("content", "itemNotPublished"));
                yield break;
            }

            // build a list of URLs, for the back-office
            // which will contain
            // - the 'main' URLs, which is what .Url would return, for each culture
            // - the 'other' URLs we know (based upon domains, etc)
            //
            // need to work through each installed culture:
            // on invariant nodes, each culture returns the same URL segment but,
            // we don't know if the branch to this content is invariant, so we need to ask
            // for URLs for all cultures.
            // and, not only for those assigned to domains in the branch, because we want
            // to show what GetUrl() would return, for every culture.

            var urls = new HashSet<UrlInfo>();
            var cultures = localizationService.GetAllLanguages().Select(x => x.IsoCode).ToList();

            //get all URLs for all cultures
            //in a HashSet, so de-duplicates too
            foreach (var cultureUrl in GetContentUrlsByCulture(content, cultures, publishedRouter, umbracoContext, contentService, textService, logger))
            {
                urls.Add(cultureUrl);
            }

            //return the real URLs first, then the messages
            foreach (var urlGroup in urls.GroupBy(x => x.IsUrl).OrderByDescending(x => x.Key))
            {
                //in some cases there will be the same URL for multiple cultures:
                // * The entire branch is invariant
                // * If there are less domain/cultures assigned to the branch than the number of cultures/languages installed

                foreach (var dUrl in urlGroup.DistinctBy(x => x.Text.ToUpperInvariant()).OrderBy(x => x.Text).ThenBy(x => x.Culture))
                    yield return dUrl;
            }

            // get the 'other' URLs - ie not what you'd get with GetUrl() but URLs that would route to the document, nevertheless.
            // for these 'other' URLs, we don't check whether they are routable, collide, anything - we just report them.
            foreach (var otherUrl in umbracoContext.UrlProvider.GetOtherUrls(content.Id).OrderBy(x => x.Text).ThenBy(x => x.Culture))
                if (urls.Add(otherUrl)) //avoid duplicates
                    yield return otherUrl;
        }

        /// <summary>
        /// Tries to return a <see cref="UrlInfo"/> for each culture for the content while detecting collisions/errors
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cultures"></param>
        /// <param name="publishedRouter"></param>
        /// <param name="umbracoContext"></param>
        /// <param name="contentService"></param>
        /// <param name="textService"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static IEnumerable<UrlInfo> GetContentUrlsByCulture(IContent content,
            IEnumerable<string> cultures,
            IPublishedRouter publishedRouter,
            UmbracoContext umbracoContext,
            IContentService contentService,
            ILocalizedTextService textService,
            ILogger logger)
        {
            foreach (var culture in cultures)
            {
                // if content is variant, and culture is not published, skip
                if (content.ContentType.VariesByCulture() && !content.IsCulturePublished(culture))
                    continue;

                // if it's variant and culture is published, or if it's invariant, proceed

                string url;
                try
                {
                    url = umbracoContext.UrlProvider.GetUrl(content.Id, culture: culture);
                }
                catch (Exception ex)
                {
                    logger.Error<UrlProvider>(ex, "GetUrl exception.");
                    url = "#ex";
                }

                switch (url)
                {
                    // deal with 'could not get the URL'
                    case "#":
                        yield return HandleCouldNotGetUrl(content, culture, contentService, textService);
                        break;

                    // deal with exceptions
                    case "#ex":
                        yield return UrlInfo.Message(textService.Localize("content", "getUrlException"), culture);
                        break;

                    // got a URL, deal with collisions, add URL
                    default:
                        if (DetectCollision(logger, content, url, culture, umbracoContext, publishedRouter, textService, out var urlInfo)) // detect collisions, etc
                        {
                            yield return urlInfo;
                        }
                        else
                        {
                            yield return UrlInfo.Url(url, culture);
                        }
                        break;
                }
            }
        }

        private static UrlInfo HandleCouldNotGetUrl(IContent content, string culture, IContentService contentService, ILocalizedTextService textService)
        {
            // document has a published version yet its URL is "#" => a parent must be
            // unpublished, walk up the tree until we find it, and report.
            var parent = content;
            do
            {
                parent = parent.ParentId > 0 ? contentService.GetParent(parent) : null;
            }
            while (parent != null && parent.Published && (!parent.ContentType.VariesByCulture() || parent.IsCulturePublished(culture)));

            if (parent == null) // oops, internal error
            {
                return UrlInfo.Message(textService.Localize("content", "parentNotPublishedAnomaly"), culture);
            }

            else if (!parent.Published) // totally not published
            {
                return UrlInfo.Message(textService.Localize("content", "parentNotPublished", new[] { parent.Name }), culture);
            }

            else
            {
                // culture not published
                return UrlInfo.Message(textService.Localize("content", "parentCultureNotPublished", new[] { parent.Name }), culture);
            }
        }

        private static bool DetectCollision(ILogger logger, IContent content, string url, string culture, UmbracoContext umbracoContext, IPublishedRouter publishedRouter, ILocalizedTextService textService, out UrlInfo urlInfo)
        {
            // test for collisions on the 'main' URL
            var uri = new Uri(url.TrimEnd(Constants.CharArrays.ForwardSlash), UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri == false) uri = uri.MakeAbsolute(umbracoContext.CleanedUmbracoUrl);
            uri = UriUtility.UriToUmbraco(uri);
            var pcr = publishedRouter.CreateRequest(umbracoContext, uri);
            publishedRouter.TryRouteRequest(pcr);

            urlInfo = null;

            if (pcr.HasPublishedContent == false)
            {
                var logMsg = nameof(DetectCollision) + " did not resolve a content item for original url: {Url}, translated to {TranslatedUrl} and culture: {Culture}";
                if (pcr.IgnorePublishedContentCollisions)
                {
                    logger.Debug(typeof(UrlProviderExtensions), logMsg, url, uri, culture);
                }
                else
                {
                    logger.Warn(typeof(UrlProviderExtensions), logMsg, url, uri, culture);
                }

                urlInfo = UrlInfo.Message(textService.Localize("content", "routeErrorCannotRoute"), culture);
                return true;
            }

            if (pcr.IgnorePublishedContentCollisions)
                return false;

            if (pcr.PublishedContent.Id != content.Id)
            {
                var o = pcr.PublishedContent;
                var l = new List<string>();
                while (o != null)
                {
                    l.Add(o.Name());
                    o = o.Parent;
                }
                l.Reverse();
                var s = "/" + string.Join("/", l) + " (id=" + pcr.PublishedContent.Id + ")";

                 urlInfo = UrlInfo.Message(textService.Localize("content", "routeError", new[] { s }), culture);
                return true;
            }

            // no collision
            return false;
        }
    }
}
