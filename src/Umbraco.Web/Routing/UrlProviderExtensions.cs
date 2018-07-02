using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Logging;
using LightInject;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Routing
{
    internal static class UrlProviderExtensions
    {
        /// <summary>
        /// Gets the URLs for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="umbracoContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use this when displaying URLs, if there are errors genertaing the urls the urls themselves will
        /// contain the errors.
        /// </remarks>
        public static IEnumerable<string> GetContentUrls(this IContent content, UrlProvider urlProvider, ILocalizedTextService textService, IContentService contentService, ILogger logger)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (urlProvider == null) throw new ArgumentNullException(nameof(urlProvider));
            if (textService == null) throw new ArgumentNullException(nameof(textService));
            if (contentService == null) throw new ArgumentNullException(nameof(contentService));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            var urls = new HashSet<string>();

            if (content.Published == false)
            {
                urls.Add(textService.Localize("content/itemNotPublished"));
                return urls;
            }

            string url;
            try
            {
                url = urlProvider.GetUrl(content.Id);
            }
            catch (Exception e)
            {
                logger.Error<UrlProvider>("GetUrl exception.", e);
                url = "#ex";
            }
            if (url == "#")
            {
                // document as a published version yet it's url is "#" => a parent must be
                // unpublished, walk up the tree until we find it, and report.
                var parent = content;
                do
                {
                    parent = parent.ParentId > 0 ? parent.Parent(contentService) : null;
                }
                while (parent != null && parent.Published);

                urls.Add(parent == null
                    ? textService.Localize("content/parentNotPublishedAnomaly") // oops - internal error
                    : textService.Localize("content/parentNotPublished", new[] { parent.Name }));
            }
            else if (url == "#ex")
            {
                urls.Add(textService.Localize("content/getUrlException"));
            }
            else
            {
                // test for collisions
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
                    foreach(var otherUrl in urlProvider.GetOtherUrls(content.Id))
                    {
                        urls.Add(otherUrl);
                    }
                }
            }
            return urls;
        }
    }
}
