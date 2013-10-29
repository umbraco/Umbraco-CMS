using System.Collections.Generic;
using Umbraco.Core.Models;
using umbraco;

namespace Umbraco.Web.Routing
{
    internal static class UrlProviderExtensions
    {
        /// <summary>
        /// Gets the URLs for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// Use this when displaying URLs, if there are errors genertaing the urls the urls themselves will
        /// contain the errors.
        /// </remarks>
        public static IEnumerable<string> GetContentUrls(this IContent content)
        {
            var urls = new List<string>();

            if (content.HasPublishedVersion() == false)
            {
                urls.Add(ui.Text("content", "itemNotPublished", UmbracoContext.Current.Security.CurrentUser));
                return urls;
            }

            var urlProvider = UmbracoContext.Current.RoutingContext.UrlProvider;
            var url = urlProvider.GetUrl(content.Id);            
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
                    urls.Add(ui.Text("content", "parentNotPublishedAnomaly", UmbracoContext.Current.Security.CurrentUser));
                else
                    urls.Add(ui.Text("content", "parentNotPublished", parent.Name, UmbracoContext.Current.Security.CurrentUser));
            }
            else
            {
                urls.Add(url);
                urls.AddRange(urlProvider.GetOtherUrls(content.Id));
            }
            return urls;
        }
    }
}