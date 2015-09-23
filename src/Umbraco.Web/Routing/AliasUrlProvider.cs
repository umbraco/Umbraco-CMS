using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls using the <c>umbracoUrlAlias</c> property.
    /// </summary>
    public class AliasUrlProvider : IUrlProvider
    {
        // note - at the moment we seem to accept pretty much anything as an alias
        // without any form of validation ... could even prob. kill the XPath ...
        // ok, this is somewhat experimental and is NOT enabled by default

        #region GetUrl

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="mode">The url mode.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on url indicated by <c>current</c> and settings, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it should return <c>null</c>.</para>
        /// </remarks>
        public string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode)
        {
            return null; // we have nothing to say
        }

        #endregion

        #region GetOtherUrls

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        public IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            if (!FindByUrlAliasEnabled)
                return Enumerable.Empty<string>(); // we have nothing to say

            var node = umbracoContext.ContentCache.GetById(id);
            string umbracoUrlName = null;
            if (node.HasProperty(Constants.Conventions.Content.UrlAlias))
                umbracoUrlName = node.GetPropertyValue<string>(Constants.Conventions.Content.UrlAlias);
            if (string.IsNullOrWhiteSpace(umbracoUrlName))
                return Enumerable.Empty<string>();

            var domainHelper = new DomainHelper(umbracoContext.Application.Services.DomainService);

            var n = node;
            var domainUris = domainHelper.DomainsForNode(n.Id, current, false);
            while (domainUris == null && n != null) // n is null at root
            {
                // move to parent node
                n = n.Parent;
                domainUris = n == null ? null : domainHelper.DomainsForNode(n.Id, current, false);
            }

            var path = "/" + umbracoUrlName;

            if (domainUris == null)
            {
                var uri = new Uri(path, UriKind.Relative);
                return new[] { UriUtility.UriFromUmbraco(uri).ToString() };
            }

            return domainUris
                .Select(domainUri => new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path)))
                .Select(uri => UriUtility.UriFromUmbraco(uri).ToString());
        }

        #endregion

        #region Utilities

        private static bool FindByUrlAliasEnabled
        {
            get
            {
                // finder
                if (ContentFinderResolver.Current.ContainsType<ContentFinderByUrlAlias>())
                    return true;

                // handler wrapped into a finder
                if (ContentFinderResolver.Current.ContainsType<ContentFinderByNotFoundHandler<global::umbraco.SearchForAlias>>())
                    return true;

                // handler wrapped into special finder
                if (ContentFinderResolver.Current.ContainsType<ContentFinderByNotFoundHandlers>()
                    && NotFoundHandlerHelper.IsNotFoundHandlerEnabled<global::umbraco.SearchForAlias>())
                    return true;

                // anything else, we can't detect
                return false;
            }
        }

        string CombinePaths(string path1, string path2)
        {
            string path = path1.TrimEnd('/') + path2;
            return path == "/" ? path : path.TrimEnd('/');
        }

        #endregion
    }
}
