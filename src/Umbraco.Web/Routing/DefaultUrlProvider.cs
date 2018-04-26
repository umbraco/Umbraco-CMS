using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
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

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="mode">The url mode.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
        /// <para>If the provider is unable to provide a url, it should return <c>null</c>.</para>
        /// </remarks>
        public virtual string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode, CultureInfo culture = null)
        {
            if (!current.IsAbsoluteUri)
                throw new ArgumentException("Current url must be absolute.", "current");

            // will not use cache if previewing
            var route = umbracoContext.ContentCache.GetRouteById(id, culture);

            return GetUrlFromRoute(route, umbracoContext, id, current, mode);
        }

        internal string GetUrlFromRoute(string route, UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                _logger.Debug<DefaultUrlProvider>(() =>
                    $"Couldn't find any page with nodeId={id}. This is most likely caused by the page not being published.");
                return null;
            }

            var domainHelper = umbracoContext.GetDomainHelper(_siteDomainHelper);

            // extract domainUri and path
            // route is /<path> or <domainRootId>/<path>
            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var domainUri = pos == 0
                ? null
                : domainHelper.DomainForNode(int.Parse(route.Substring(0, pos)), current);

            // assemble the url from domainUri (maybe null) and path
            return AssembleUrl(domainUri, path, current, mode).ToString();
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
        public virtual IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            var node = umbracoContext.ContentCache.GetById(id);
            var domainHelper = umbracoContext.GetDomainHelper(_siteDomainHelper);

            var n = node;
            var domainUris = domainHelper.DomainsForNode(n.Id, current, false);
            while (domainUris == null && n != null) // n is null at root
            {
                // move to parent node
                n = n.Parent;
                domainUris = n == null ? null : domainHelper.DomainsForNode(n.Id, current);
            }

            if (domainUris == null)
            {
                //there are no domains, exit
                return Enumerable.Empty<string>();
            }

            var result = new List<string>();
            foreach (var d in domainUris)
            {
                //although we are passing in culture here, if any node in this path is invariant, it ignores the culture anyways so this is ok
                var route = umbracoContext.ContentCache.GetRouteById(id, d.Culture);
                if (route == null) continue;

                //need to strip off the leading ID for the route
                //TODO: Is there a nicer way to deal with this?
                var pos = route.IndexOf('/');
                var path = pos == 0 ? route : route.Substring(pos);

                var uri = new Uri(CombinePaths(d.Uri.GetLeftPart(UriPartial.Path), path));
                uri = UriUtility.UriFromUmbraco(uri, _globalSettings, _requestSettings);
                result.Add(uri.ToString());
            }
            return result;
        }

        #endregion

        #region Utilities

        Uri AssembleUrl(DomainAndUri domainUri, string path, Uri current, UrlProviderMode mode)
        {
            Uri uri;

            // ignore vdir at that point, UriFromUmbraco will do it

            if (mode == UrlProviderMode.AutoLegacy)
            {
                mode = _requestSettings.UseDomainPrefixes
                    ? UrlProviderMode.Absolute
                    : UrlProviderMode.Auto;
            }

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
                        throw new ArgumentOutOfRangeException("mode");
                }
            }
            else // a domain was found
            {
                if (mode == UrlProviderMode.Auto)
                {
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
                        throw new ArgumentOutOfRangeException("mode");
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
