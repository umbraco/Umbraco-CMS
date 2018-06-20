using System;
using System.Collections.Generic;
using System.Threading;
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
        public static IEnumerable<string> GetContentUrls(this IContent content, UrlProvider urlProvider, ILocalizedTextService textService, IContentService contentService, ILogger logger)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (urlProvider == null) throw new ArgumentNullException(nameof(urlProvider));
            if (textService == null) throw new ArgumentNullException(nameof(textService));
            if (contentService == null) throw new ArgumentNullException(nameof(contentService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            var urls = new HashSet<string>();

            // going to build a list of urls (essentially for the back-office)
            // which will contain
            // - the 'main' url, which is what .Url would return, in the current culture
            // - the 'other' urls we know (based upon domains, etc)
            //
            // this essentially happens when producing the urls for the back-office, and then we don't have
            // a meaningful 'current culture' - so we need to explicitely pass some culture where required,
            // and deal with whatever might happen
            //
            // if content is variant, go with the current culture - and that is NOT safe, there may be
            // no 'main' url for that culture, deal with it later - otherwise, go with the invariant
            // culture, and that is safe.
            var varies = content.ContentType.VariesByCulture();
            var culture = varies ? Thread.CurrentThread.CurrentUICulture.Name : "";

            if (content.Published == false)
            {
                urls.Add(textService.Localize("content/itemNotPublished"));
                return urls;
            }

            string url = null;

            if (varies)
            {
                if (!content.IsCulturePublished(culture))
                {
                    urls.Add(textService.Localize("content/itemCultureNotPublished", culture));
                    // but keep going, we want to add the 'other' urls
                    url = "#no";
                }
                else if (!content.IsCultureAvailable(culture))
                {
                    urls.Add(textService.Localize("content/itemCultureNotAvailable", culture));
                    // but keep going, we want to add the 'other' urls
                    url = "#no";
                }
            }

            // get the 'main' url
            if (url == null)
            {
                try
                {
                    url = urlProvider.GetUrl(content.Id, culture);
                }
                catch (Exception e)
                {
                    logger.Error<UrlProvider>("GetUrl exception.", e);
                    url = "#ex";
                }
            }

            if (url == "#") // deal with 'could not get the url'
            {
                // document as a published version yet its url is "#" => a parent must be
                // unpublished, walk up the tree until we find it, and report.
                var parent = content;
                do
                {
                    parent = parent.ParentId > 0 ? parent.Parent(contentService) : null;
                }
                while (parent != null && parent.Published && (!varies || parent.IsCulturePublished(culture)));

                urls.Add(parent == null
                    ? textService.Localize("content/parentNotPublishedAnomaly") // oops - internal error
                    : textService.Localize("content/parentNotPublished", new[] { parent.Name }));
            }
            else if (url == "#ex") // deal with exceptions
            {
                urls.Add(textService.Localize("content/getUrlException"));
            }
            else if (url == "#no") // deal with 'there is no main url'
            {
                // get the 'other' urls
                foreach(var otherUrl in urlProvider.GetOtherUrls(content.Id))
                    urls.Add(otherUrl);
            }
            else // detect collisions, etc
            {
                // test for collisions on the 'main' url
                var uri = new Uri(url.TrimEnd('/'), UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri == false) uri = uri.MakeAbsolute(UmbracoContext.Current.CleanedUmbracoUrl);
                uri = UriUtility.UriToUmbraco(uri);
                var r = Core.Composing.Current.Container.GetInstance<PublishedRouter>(); // fixme inject or ?
                var pcr = r.CreateRequest(UmbracoContext.Current, uri);
                r.TryRouteRequest(pcr);

                if (pcr.HasPublishedContent == false)
                {
                    urls.Add(textService.Localize("content/routeError", new[] { "(error)" }));
                }
                else if (pcr.PublishedContent.Id != content.Id)
                {
                    var o = pcr.PublishedContent;
                    string s;
                    if (o == null)
                    {
                        s = "(unknown)";
                    }
                    else
                    {
                        var l = new List<string>();
                        while (o != null)
                        {
                            l.Add(o.Name);
                            o = o.Parent;
                        }
                        l.Reverse();
                        s = "/" + string.Join("/", l) + " (id=" + pcr.PublishedContent.Id + ")";

                    }
                    urls.Add(textService.Localize("content/routeError", s));
                }
                else
                {
                    urls.Add(url);

                    // get the 'other' urls
                    foreach(var otherUrl in urlProvider.GetOtherUrls(content.Id))
                        urls.Add(otherUrl);
                }
            }
            return urls;
        }
    }
}
