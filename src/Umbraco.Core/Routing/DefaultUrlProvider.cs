using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    public class DefaultUrlProvider : IUrlProvider
    {
        private readonly RequestHandlerSettings _requestSettings;
        private readonly ILogger<DefaultUrlProvider> _logger;
        private readonly ISiteDomainHelper _siteDomainHelper;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly UriUtility _uriUtility;

        public DefaultUrlProvider(IOptions<RequestHandlerSettings> requestSettings, ILogger<DefaultUrlProvider> logger, ISiteDomainHelper siteDomainHelper, IUmbracoContextAccessor umbracoContextAccessor, UriUtility uriUtility)
        {
            _requestSettings = requestSettings.Value;
            _logger = logger;
            _siteDomainHelper = siteDomainHelper;
            _uriUtility = uriUtility;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        #region GetUrl

        /// <inheritdoc />
        public virtual UrlInfo GetUrl(IPublishedContent content, UrlMode mode, string culture, Uri current)
        {
            if (!current.IsAbsoluteUri) throw new ArgumentException("Current URL must be absolute.", nameof(current));

            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            // will not use cache if previewing
            var route = umbracoContext.Content.GetRouteById(content.Id, culture);

            return GetUrlFromRoute(route, umbracoContext, content.Id, current, mode, culture);
        }

        internal UrlInfo GetUrlFromRoute(string route, IUmbracoContext umbracoContext, int id, Uri current, UrlMode mode, string culture)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                _logger.LogDebug("Couldn't find any page with nodeId={NodeId}. This is most likely caused by the page not being published.", id);
                return null;
            }

            // extract domainUri and path
            // route is /<path> or <domainRootId>/<path>
            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var domainUri = pos == 0
                ? null
                : DomainUtilities.DomainForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainHelper, int.Parse(route.Substring(0, pos)), current, culture);

            // assemble the URL from domainUri (maybe null) and path
            var url = AssembleUrl(domainUri, path, current, mode).ToString();

            return UrlInfo.Url(url, culture);
        }

        #endregion

        #region GetOtherUrls

        /// <summary>
        /// Gets the other URLs of a published content.
        /// </summary>
        /// <param name="umbracoContextAccessor">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute URL.</param>
        /// <returns>The other URLs for the published content.</returns>
        /// <remarks>
        /// <para>Other URLs are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// URLs for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        public virtual IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            var node = umbracoContext.Content.GetById(id);
            if (node == null)
            {
                yield break;
            }

            // look for domains, walking up the tree
            var n = node;
            var domainUris = DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainHelper, n.Id, current, false);
            while (domainUris == null && n != null) // n is null at root
            {
                n = n.Parent; // move to parent node
                domainUris = n == null ? null : DomainUtilities.DomainsForNode(umbracoContext.PublishedSnapshot.Domains, _siteDomainHelper, n.Id, current, excludeDefault: true);
            }

            // no domains = exit
            if (domainUris ==null)
            {
                yield break;
            }

            foreach (var d in domainUris)
            {
                var culture = d?.Culture;

                // although we are passing in culture here, if any node in this path is invariant, it ignores the culture anyways so this is ok
                var route = umbracoContext.Content.GetRouteById(id, culture);
                if (route == null) continue;

                // need to strip off the leading ID for the route if it exists (occurs if the route is for a node with a domain assigned)
                var pos = route.IndexOf('/');
                var path = pos == 0 ? route : route.Substring(pos);

                var uri = new Uri(CombinePaths(d.Uri.GetLeftPart(UriPartial.Path), path));
                uri = _uriUtility.UriFromUmbraco(uri, _requestSettings);
                yield return UrlInfo.Url(uri.ToString(), culture);
            }
        }

        #endregion

        #region Utilities

        Uri AssembleUrl(DomainAndUri domainUri, string path, Uri current, UrlMode mode)
        {
            Uri uri;

            // ignore vdir at that point, UriFromUmbraco will do it

            if (domainUri == null) // no domain was found
            {
                if (current == null)
                    mode = UrlMode.Relative; // best we can do

                switch (mode)
                {
                    case UrlMode.Absolute:
                        uri = new Uri(current.GetLeftPart(UriPartial.Authority) + path);
                        break;
                    case UrlMode.Relative:
                    case UrlMode.Auto:
                        uri = new Uri(path, UriKind.Relative);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }
            else // a domain was found
            {
                if (mode == UrlMode.Auto)
                {
                    //this check is a little tricky, we can't just compare domains
                    if (current != null && domainUri.Uri.GetLeftPart(UriPartial.Authority) == current.GetLeftPart(UriPartial.Authority))
                        mode = UrlMode.Relative;
                    else
                        mode = UrlMode.Absolute;
                }

                switch (mode)
                {
                    case UrlMode.Absolute:
                        uri = new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path));
                        break;
                    case UrlMode.Relative:
                        uri = new Uri(CombinePaths(domainUri.Uri.AbsolutePath, path), UriKind.Relative);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }

            // UriFromUmbraco will handle vdir
            // meaning it will add vdir into domain URLs too!
            return _uriUtility.UriFromUmbraco(uri, _requestSettings);
        }

        string CombinePaths(string path1, string path2)
        {
            string path = path1.TrimEnd(Constants.CharArrays.ForwardSlash) + path2;
            return path == "/" ? path : path.TrimEnd(Constants.CharArrays.ForwardSlash);
        }

        #endregion
    }
}
