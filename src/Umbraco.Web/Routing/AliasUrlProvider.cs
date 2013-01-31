using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls using the <c>umbracoUrlAlias</c> property.
    /// </summary>
    internal class AliasUrlProvider : IUrlProvider
    {
        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="contentCache">The content cache.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on url indicated by <c>current</c> and settings, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it should return <c>null</c>.</para>
        /// </remarks>
        public string GetUrl(UmbracoContext umbracoContext, IPublishedContentStore contentCache, int id, Uri current, bool absolute)
        {
            return null; // we have nothing to say
        }

        const string UmbracoUrlAlias = "umbracoUrlAlias";

        private bool FindByUrlAliasEnabled
        {
            get
            {
                var hasFinder = ContentFinderResolver.Current.ContainsType<ContentFinderByUrlAlias>();
                var hasHandler = ContentFinderResolver.Current.ContainsType<ContentFinderByNotFoundHandlers>()
                    && NotFoundHandlerHelper.CustomHandlerTypes.Contains(typeof(global::umbraco.SearchForAlias));
                return hasFinder || hasHandler;
            }
        }

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="contentCache">The content cache.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        public IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, IPublishedContentStore contentCache, int id, Uri current)
        {
            if (!FindByUrlAliasEnabled)
                return Enumerable.Empty<string>(); // we have nothing to say

            var node = contentCache.GetDocumentById(umbracoContext, id);
            string umbracoUrlName = null;
            if (node.HasProperty(UmbracoUrlAlias))
                umbracoUrlName = node.GetPropertyValue<string>(UmbracoUrlAlias);
            if (string.IsNullOrWhiteSpace(umbracoUrlName))
                return Enumerable.Empty<string>();

            /*
            
            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting urls in the way
            var n = node;
            Uri domainUri = DomainUriAtNode(n.Id, current);
            while (domainUri == null && n != null) // n is null at root
            {
                // get the url
                var urlName = n.UrlName;

                // move to parent node
                n = n.Parent;
                domainUri = n == null ? null : DomainUriAtNode(n.Id, current);
            }

            if (domainUri == null)
                return new string[] { "/" + umbracoUrlName };
            else
                // fuck - there may be MANY domains actually!!
                return null;
             * 
             */

            // just for fun
            return new[] { "/" + umbracoUrlName };
        }
    }
}
