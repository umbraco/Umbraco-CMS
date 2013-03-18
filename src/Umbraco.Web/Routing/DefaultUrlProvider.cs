using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    internal class DefaultUrlProvider : IUrlProvider
    {
        #region GetUrl

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="contentCache">The content cache.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="mode">The url mode.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
        /// <para>If the provider is unable to provide a url, it should return <c>null</c>.</para>
        /// </remarks>
        public virtual string GetUrl(UmbracoContext umbracoContext, IPublishedContentStore contentCache, int id, Uri current, UrlProviderMode mode)
        {
            DomainAndUri domainUri;
            string path;

            if (!current.IsAbsoluteUri)
// ReSharper disable LocalizableElement
                throw new ArgumentException("Current url must be absolute.", "current");
// ReSharper restore LocalizableElement

            // do not read cache if previewing
            var route = umbracoContext.InPreviewMode
                ? null
                : umbracoContext.RoutingContext.RoutesCache.GetRoute(id);

            if (!string.IsNullOrEmpty(route))
            {
                // there was a route in the cache - extract domainUri and path
                // route is /<path> or <domainRootId>/<path>
                var pos = route.IndexOf('/');
                path = pos == 0 ? route : route.Substring(pos);
                domainUri = pos == 0 ? null : DomainHelper.DomainForNode(int.Parse(route.Substring(0, pos)), current);
            }
            else
            {
                // there was no route in the cache - create a route
                var node = contentCache.GetDocumentById(umbracoContext, id);
                if (node == null)
                {
                    LogHelper.Warn<DefaultUrlProvider>(
                        "Couldn't find any page with nodeId={0}. This is most likely caused by the page not being published.",
                        () => id);

                    return null;
                }

                // walk up from that node until we hit a node with a domain,
                // or we reach the content root, collecting urls in the way
                var pathParts = new List<string>();
                var n = node;
                domainUri = DomainHelper.DomainForNode(n.Id, current);
                while (domainUri == null && n != null) // n is null at root
                {
                    // get the url
                    var urlName = n.UrlName;
                    pathParts.Add(urlName);

                    // move to parent node
                    n = n.Parent;
                    domainUri = n == null ? null : DomainHelper.DomainForNode(n.Id, current);
                }

                // no domain, respect HideTopLevelNodeFromPath for legacy purposes
                if (domainUri == null && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
                    ApplyHideTopLevelNodeFromPath(umbracoContext, contentCache, node, pathParts);

                // assemble the route
                pathParts.Reverse();
                path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
                route = (n == null ? "" : n.Id.ToString()) + path;

                // do not store if previewing
                if (!umbracoContext.InPreviewMode)
                    umbracoContext.RoutingContext.RoutesCache.Store(id, route);
            }

            // assemble the url from domainUri (maybe null) and path
            return AssembleUrl(domainUri, path, current, mode).ToString();
        }

        #endregion

        #region GetOtherUrls

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
        public virtual IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, IPublishedContentStore contentCache, int id, Uri current)
        {
            string path;
            IEnumerable<DomainAndUri> domainUris;

            // will not read cache if previewing!
            var route = umbracoContext.InPreviewMode
                ? null
                : umbracoContext.RoutingContext.RoutesCache.GetRoute(id);

            if (!string.IsNullOrEmpty(route))
            {
                // there was a route in the cache - extract domainUri and path
                // route is /<path> or <domainRootId>/<path>
                int pos = route.IndexOf('/');
                path = pos == 0 ? route : route.Substring(pos);
                domainUris = pos == 0 ? null : DomainHelper.DomainsForNode(int.Parse(route.Substring(0, pos)), current);
            }
            else
            {
                // there was no route in the cache - create a route
                var node = contentCache.GetDocumentById(umbracoContext, id);
                if (node == null)
                {
                    LogHelper.Warn<DefaultUrlProvider>(
                        "Couldn't find any page with nodeId={0}. This is most likely caused by the page not being published.",
                        () => id);

                    return null;
                }

                // walk up from that node until we hit a node with domains,
                // or we reach the content root, collecting urls in the way
                var pathParts = new List<string>();
                var n = node;
                domainUris = DomainHelper.DomainsForNode(n.Id, current);
                while (domainUris == null && n != null) // n is null at root
                {
                    // get the url
                    var urlName = node.UrlName;
                    pathParts.Add(urlName);

                    // move to parent node
                    n = n.Parent;
                    domainUris = n == null ? null : DomainHelper.DomainsForNode(n.Id, current);
                }

                // no domain, respect HideTopLevelNodeFromPath for legacy purposes
                if (domainUris == null && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
                    ApplyHideTopLevelNodeFromPath(umbracoContext, contentCache, node, pathParts);

                // assemble the route
                pathParts.Reverse();
                path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
                route = (n == null ? "" : n.Id.ToString()) + path;

                // do not store if previewing
                if (!umbracoContext.InPreviewMode)
                    umbracoContext.RoutingContext.RoutesCache.Store(id, route);
            }

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
                mode = Core.Configuration.UmbracoSettings.UseDomainPrefixes
                    ? UrlProviderMode.Absolute
                    : UrlProviderMode.Auto;
            }

            if (mode == UrlProviderMode.AutoLegacy)
            {
                mode = Core.Configuration.UmbracoSettings.UseDomainPrefixes
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

        static void ApplyHideTopLevelNodeFromPath(UmbracoContext umbracoContext, IPublishedContentStore contentCache, Core.Models.IPublishedContent node, IList<string> pathParts)
        {
            // in theory if hideTopLevelNodeFromPath is true, then there should be only once
            // top-level node, or else domains should be assigned. but for backward compatibility
            // we add this check - we look for the document matching "/" and if it's not us, then
            // we do not hide the top level path
            // it has to be taken care of in IPublishedContentStore.GetDocumentByRoute too so if
            // "/foo" fails (looking for "/*/foo") we try also "/foo". 
            // this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
            // that's the way it works pre-4.10 and we try to be backward compat for the time being
            if (node.Parent == null)
            {
                var rootNode = contentCache.GetDocumentByRoute(umbracoContext, "/", true);
                if (rootNode.Id == node.Id) // remove only if we're the default node
                    pathParts.RemoveAt(pathParts.Count - 1);
            }
            else
            {
                pathParts.RemoveAt(pathParts.Count - 1);
            }
        }

        #endregion
    }
}
