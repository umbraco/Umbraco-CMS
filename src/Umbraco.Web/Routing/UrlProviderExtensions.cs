using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using LightInject;

namespace Umbraco.Web.Routing
{
    internal static class UrlProviderExtensions
    {
        /// <summary>
        /// Gets the Urls of the content item.
        /// </summary>
        /// <remarks>
        /// <para>Use when displaying Urls. If errors occur when generating the Urls, they will show in the list.</para>
        /// <para>Contains all the Urls that we can figure out (based upon domains, etc).</para>
        /// </remarks>
        public static IEnumerable<UrlInfo> GetContentUrls(this IContent content, UrlProvider urlProvider, ILocalizedTextService textService, IContentService contentService, ILogger logger)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (urlProvider == null) throw new ArgumentNullException(nameof(urlProvider));
            if (textService == null) throw new ArgumentNullException(nameof(textService));
            if (contentService == null) throw new ArgumentNullException(nameof(contentService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            var urls = new List<UrlInfo>();

            if (content.Published == false)
            {
                urls.Add(UrlInfo.Message(textService.Localize("content/itemNotPublished")));
                return urls;
            }

            // fixme inject
            // fixme PublishedRouter is stateless and should be a singleton!
            var localizationService = Core.Composing.Current.Services.LocalizationService;
            var publishedRouter = Core.Composing.Current.Container.GetInstance<PublishedRouter>();

            // build a list of urls, for the back-office
            // which will contain
            // - the 'main' urls, which is what .Url would return, for each culture
            // - the 'other' urls we know (based upon domains, etc)
            //
            // need to work on each culture.
            // on invariant trees, each culture return the same thing
            // but, we don't know if the tree to this content is invariant

            var cultures = localizationService.GetAllLanguages().Select(x => x.IsoCode).ToList();

            foreach (var culture in cultures)
            {
                // if content is variant, and culture is not published, skip
                if (content.ContentType.VariesByCulture() && !content.IsCulturePublished(culture))
                    continue;

                // if it's variant and culture is published, or if it's invariant, proceed

                string url;
                try
                {
                    url = urlProvider.GetUrl(content.Id, culture);
                }
                catch (Exception e)
                {
                    logger.Error<UrlProvider>("GetUrl exception.", e);
                    url = "#ex";
                }

                switch (url)
                {
                    // deal with 'could not get the url'
                    case "#":
                        HandleCouldNotGetUrl(content, culture, urls, contentService, textService);
                        break;

                    // deal with exceptions
                    case "#ex":
                        urls.Add(UrlInfo.Message(textService.Localize("content/getUrlException"), culture));
                        break;

                    // got a url, deal with collisions, add url
                    default:
                        if (!DetectCollision(content, url, urls, culture, publishedRouter, textService)) // detect collisions, etc
                            urls.Add(UrlInfo.Url(url, culture));
                        break;
                }
            }

            // prepare for de-duplication
            var durl = new Dictionary<string, List<UrlInfo>>();
            var dmsg = new Dictionary<string, List<UrlInfo>>();
            foreach (var url in urls)
            {
                var d = url.IsUrl ? durl : dmsg;
                if (!d.TryGetValue(url.Text, out var l))
                    d[url.Text] = l = new List<UrlInfo>();
                l.Add(url);
            }

            // deduplicate, order urls first then messages, concatenate cultures (hide if 'all')
            var ret = new List<UrlInfo>();
            foreach (var (text, infos) in durl)
                ret.Add(UrlInfo.Url(text, infos.Count == cultures.Count ?  null : string.Join(", ", infos.Select(x => x.Culture))));
            foreach (var (text, infos) in dmsg)
                ret.Add(UrlInfo.Message(text, infos.Count == cultures.Count ?  null : string.Join(", ", infos.Select(x => x.Culture))));

            // fixme - need to add 'others' urls
            // but, when?
            //// get the 'other' urls
            //foreach(var otherUrl in urlProvider.GetOtherUrls(content.Id))
            //    urls.Add(otherUrl);

            return ret;
        }

        private static void HandleCouldNotGetUrl(IContent content, string culture, List<UrlInfo> urls, IContentService contentService, ILocalizedTextService textService)
        {
            // document has a published version yet its url is "#" => a parent must be
            // unpublished, walk up the tree until we find it, and report.
            var parent = content;
            do
            {
                parent = parent.ParentId > 0 ? parent.Parent(contentService) : null;
            }
            while (parent != null && parent.Published && (!parent.ContentType.VariesByCulture() || parent.IsCulturePublished(culture)));

            if (parent == null) // oops, internal error
                urls.Add(UrlInfo.Message(textService.Localize("content/parentNotPublishedAnomaly"), culture));

            else if (!parent.Published) // totally not published
                urls.Add(UrlInfo.Message(textService.Localize("content/parentNotPublished", new[] { parent.Name }), culture));

            else // culture not published
                urls.Add(UrlInfo.Message(textService.Localize("content/parentCultureNotPublished", new[] { parent.Name }), culture));
        }

        private static bool DetectCollision(IContent content, string url, List<UrlInfo> urls, string culture, PublishedRouter publishedRouter, ILocalizedTextService textService)
        {
            // test for collisions on the 'main' url
            var uri = new Uri(url.TrimEnd('/'), UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri == false) uri = uri.MakeAbsolute(UmbracoContext.Current.CleanedUmbracoUrl);
            uri = UriUtility.UriToUmbraco(uri);
            var pcr = publishedRouter.CreateRequest(UmbracoContext.Current, uri);
            publishedRouter.TryRouteRequest(pcr);

            if (pcr.HasPublishedContent == false)
            {
                urls.Add(UrlInfo.Message(textService.Localize("content/routeErrorCannotRoute"), culture));
                return true;
            }

            if (pcr.PublishedContent.Id != content.Id)
            {
                var o = pcr.PublishedContent;
                var l = new List<string>();
                while (o != null)
                {
                    l.Add(o.Name);
                    o = o.Parent;
                }
                l.Reverse();
                var s = "/" + string.Join("/", l) + " (id=" + pcr.PublishedContent.Id + ")";

                urls.Add(UrlInfo.Message(textService.Localize("content/routeError", s), culture));
                return true;
            }

            // no collision
            return false;
        }
    }
}
