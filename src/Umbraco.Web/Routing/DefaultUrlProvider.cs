using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    public class DefaultUrlProvider : IUrlProvider
    {
        private readonly IRequestHandlerSection _requestSettings;
        private readonly ILogger _logger;
        private readonly IGlobalSettings _globalSettings;
        private readonly ISiteDomainHelper _siteDomainHelper;

        public DefaultUrlProvider(IRequestHandlerSection requestSettings, ILogger logger, IGlobalSettings globalSettings, ISiteDomainHelper siteDomainHelper)
        {
            _requestSettings = requestSettings;
            _logger = logger;
            _globalSettings = globalSettings;
            _siteDomainHelper = siteDomainHelper;
        }

        #region GetUrl

        /// <inheritdoc />
        public virtual UrlInfo GetUrl(UmbracoContext umbracoContext, IPublishedContent content, UrlProviderMode mode, string culture, Uri current)
        {
            if (!current.IsAbsoluteUri) throw new ArgumentException("Current url must be absolute.", nameof(current));

            // will not use cache if previewing
            var route = umbracoContext.ContentCache.GetRouteById(content.Id, culture);

            return GetUrlFromRoute(route, umbracoContext, content.Id, current, mode, culture);
        }

        internal UrlInfo GetUrlFromRoute(string route, UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode, string culture)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                _logger.Debug<DefaultUrlProvider>("Couldn't find any page with nodeId={NodeId}. This is most likely caused by the page not being published.", id);
                return null;
            }

            var domainHelper = umbracoContext.GetDomainHelper(_siteDomainHelper);

            // extract domainUri and path
            // route is /<path> or <domainRootId>/<path>
            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var domainUri = pos == 0
                ? null
                : domainHelper.DomainForNode(int.Parse(route.Substring(0, pos)), current, culture);

            // assemble the url from domainUri (maybe null) and path
            var url = AssembleUrl(domainUri, path, current, mode).ToString();

            return UrlInfo.Url(url, culture);
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
        public virtual IEnumerable<UrlInfo> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            var node = umbracoContext.ContentCache.GetById(id);
            if (node == null)
                yield break;

            var domainHelper = umbracoContext.GetDomainHelper(_siteDomainHelper);

            // look for domains, walking up the tree
            var n = node;
            var domainUris = domainHelper.DomainsForNode(n.Id, current, false);
            while (domainUris == null && n != null) // n is null at root
            {
                n = n.Parent; // move to parent node
                domainUris = n == null ? null : domainHelper.DomainsForNode(n.Id, current, excludeDefault: true);
            }

            // no domains = exit
            if (domainUris ==null)
                yield break;

            foreach (var d in domainUris)
            {
                var culture = d?.Culture?.Name;

                //although we are passing in culture here, if any node in this path is invariant, it ignores the culture anyways so this is ok
                var route = umbracoContext.ContentCache.GetRouteById(id, culture);
                if (route == null) continue;

                //need to strip off the leading ID for the route if it exists (occurs if the route is for a node with a domain assigned)
                var pos = route.IndexOf('/');
                var path = pos == 0 ? route : route.Substring(pos);

                var uri = new Uri(CombinePaths(d.Uri.GetLeftPart(UriPartial.Path), path));
                uri = UriUtility.UriFromUmbraco(uri, _globalSettings, _requestSettings);
                yield return UrlInfo.Url(uri.ToString(), culture);
            }
        }

        #endregion

        #region Utilities

        Uri AssembleUrl(DomainAndUri domainUri, string path, Uri current, UrlProviderMode mode)
        {
            Uri uri;

            // ignore vdir at that point, UriFromUmbraco will do it

            if (domainUri == null) // no domain was found
            {
                if (current == null)
                    mode = UrlProviderMode.Relative; // best we can do

                switch (mode)
                {
                    case UrlProviderMode.Absolute:
                        uri = new Uri(current.GetLeftPart(UriPartial.Authority) + path);
                        break;
                    case UrlProviderMode.Relative:
                    case UrlProviderMode.Auto:
                        uri = new Uri(path, UriKind.Relative);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }
            else // a domain was found
            {
                if (mode == UrlProviderMode.Auto)
                {
                    //this check is a little tricky, we can't just compare domains
                    if (current != null && domainUri.Uri.GetLeftPart(UriPartial.Authority) == current.GetLeftPart(UriPartial.Authority))
                        mode = UrlProviderMode.Relative;
                    else
                        mode = UrlProviderMode.Absolute;
                }

                switch (mode)
                {
                    case UrlProviderMode.Absolute:
                        uri = new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path));
                        break;
                    case UrlProviderMode.Relative:
                        uri = new Uri(CombinePaths(domainUri.Uri.AbsolutePath, path), UriKind.Relative);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }

            // UriFromUmbraco will handle vdir
            // meaning it will add vdir into domain urls too!
            return UriUtility.UriFromUmbraco(uri, _globalSettings, _requestSettings);
        }

        string CombinePaths(string path1, string path2)
        {
            string path = path1.TrimEnd('/') + path2;
            return path == "/" ? path : path.TrimEnd('/');
        }

        #endregion
    }
}
