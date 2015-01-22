using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Web.PublishedCache;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    public class DefaultUrlProvider : IUrlProvider
    {
        private readonly IRequestHandlerSection _requestSettings;

        [Obsolete("Use the ctor that specifies the IRequestHandlerSection")]
        public DefaultUrlProvider()
            : this(UmbracoConfig.For.UmbracoSettings().RequestHandler)
        {            
        }

        public DefaultUrlProvider(IRequestHandlerSection requestSettings)
        {
            _requestSettings = requestSettings;
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
        public virtual string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode)
        {
            if (!current.IsAbsoluteUri)
                throw new ArgumentException("Current url must be absolute.", "current");

            // will not use cache if previewing
            var route = umbracoContext.ContentCache.GetRouteById(id);

            if (string.IsNullOrWhiteSpace(route))
            {
                LogHelper.Debug<DefaultUrlProvider>(
                    "Couldn't find any page with nodeId={0}. This is most likely caused by the page not being published.",
                    () => id);
                return null;
            }

            var domainHelper = new DomainHelper(umbracoContext.Application.Services.DomainService);

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
            // will not use cache if previewing
            var route = umbracoContext.ContentCache.GetRouteById(id);

            if (string.IsNullOrWhiteSpace(route))
            {
                LogHelper.Debug<DefaultUrlProvider>(
                    "Couldn't find any page with nodeId={0}. This is most likely caused by the page not being published.",
                    () => id);
                return null;
            }

            var domainHelper = new DomainHelper(umbracoContext.Application.Services.DomainService);

            // extract domainUri and path
            // route is /<path> or <domainRootId>/<path>
            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var domainUris = pos == 0 ? null : domainHelper.DomainsForNode(int.Parse(route.Substring(0, pos)), current);

            // assemble the alternate urls from domainUris (maybe empty) and path
            return AssembleUrls(domainUris, path).Select(uri => uri.ToString());
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
            return UriUtility.UriFromUmbraco(uri);
        }

        string CombinePaths(string path1, string path2)
        {
            string path = path1.TrimEnd('/') + path2;
            return path == "/" ? path : path.TrimEnd('/');
        }

        // always build absolute urls unless we really cannot
        IEnumerable<Uri> AssembleUrls(IEnumerable<DomainAndUri> domainUris, string path)
        {
            // no domain == no "other" url
            if (domainUris == null)
                return Enumerable.Empty<Uri>();

            // if no domain was found and then we have no "other" url
            // else return absolute urls, ignoring vdir at that point
            var uris = domainUris.Select(domainUri => new Uri(CombinePaths(domainUri.Uri.GetLeftPart(UriPartial.Path), path)));

            // UriFromUmbraco will handle vdir
            // meaning it will add vdir into domain urls too!
            return uris.Select(UriUtility.UriFromUmbraco);
        }

        #endregion
    }
}
