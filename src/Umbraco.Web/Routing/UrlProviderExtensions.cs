using System;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Core.Models;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

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
        public static IEnumerable<string> GetContentUrls(this IContent content, UmbracoContext umbracoContext)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");

            var urls = new List<string>();

            if (content.HasPublishedVersion == false)
            {
                urls.Add(ui.Text("content", "itemNotPublished", umbracoContext.Security.CurrentUser));
                return urls;
            }

            string url;
            var urlProvider = umbracoContext.RoutingContext.UrlProvider;
            try
            {
                url = urlProvider.GetUrl(content.Id);
            }
            catch (Exception e)
            {
                LogHelper.Error<UrlProvider>("GetUrl exception.", e);
                url = "#ex";
            }
            if (url == "#")
            {
                // document as a published version yet it's url is "#" => a parent must be
                // unpublished, walk up the tree until we find it, and report.
                var parent = content;
                do
                {
                    parent = parent.ParentId > 0 ? parent.Parent() : null;
                }
                while (parent != null && parent.Published);

                if (parent == null) // oops - internal error
                    urls.Add(ui.Text("content", "parentNotPublishedAnomaly", umbracoContext.Security.CurrentUser));
                else
                    urls.Add(ui.Text("content", "parentNotPublished", parent.Name, umbracoContext.Security.CurrentUser));
            }
            else if (url == "#ex")
            {
                urls.Add(ui.Text("content", "getUrlException", umbracoContext.Security.CurrentUser));
            }
            else
            {
                // test for collisions
                var uri = new Uri(url.TrimEnd('/'), UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri == false) uri = uri.MakeAbsolute(UmbracoContext.Current.CleanedUmbracoUrl);
                uri = UriUtility.UriToUmbraco(uri);
                var pcr = new PublishedContentRequest(uri, UmbracoContext.Current.RoutingContext, UmbracoConfig.For.UmbracoSettings().WebRouting, s => Roles.Provider.GetRolesForUser(s));
                pcr.Engine.TryRouteRequest();

                if (pcr.HasPublishedContent == false)
                {
                    urls.Add(ui.Text("content", "routeError", "(error)", umbracoContext.Security.CurrentUser));
                }
                else if (pcr.IgnorePublishedContentCollisions == false && pcr.PublishedContent.Id != content.Id)
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
                    urls.Add(ui.Text("content", "routeError", s, umbracoContext.Security.CurrentUser));
                }
                else
                {
                    urls.Add(url);
                    urls.AddRange(urlProvider.GetOtherUrls(content.Id));
                }
            }
            return urls;
        }
    }
}