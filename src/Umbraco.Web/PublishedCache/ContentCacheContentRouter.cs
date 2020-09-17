using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache;
using CacheKeys = Umbraco.Web.PublishedCache.NuCache.CacheKeys;

namespace Umbraco.Web.PublishedCache
{
    internal class ContentCacheContentRouter : IContentRouter
    {
        private readonly SnapshotGetStrategy _navigableData;
        private readonly IGlobalSettings _globalSettings;
        private readonly IAppCache _snapshotCache;
        private readonly IAppCache _elementsCache;
        private readonly IDomainCache _domainCache;
        private readonly bool _previewDefault;

        public ContentCacheContentRouter(SnapshotGetStrategy snapshotByIdStrategy, IGlobalSettings globalSettings, IAppCache snapshotCache,
            IAppCache elementsCache, IDomainCache domainCache, bool previewDefault)
        {
            _navigableData = snapshotByIdStrategy;
            _globalSettings = globalSettings;
            _snapshotCache = snapshotCache;
            _elementsCache = elementsCache;
            _domainCache = domainCache;
            _previewDefault = previewDefault;
        }

        public ContentRoutingResult GetIdByRoute(string route, bool? hideTopLevelNode = null, string culture = null)
        {
            return GetIdByRoute(_previewDefault, route, hideTopLevelNode, culture);
        }

        public ContentRoutingResult GetIdByRoute(bool preview, string route, bool? hideTopLevelNode = null, string culture = null)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            var cache = preview == false || PublishedSnapshotService.FullCacheWhenPreviewing ? _elementsCache : _snapshotCache;
            var key = CacheKeys.ContentCacheContentIdByRoute(route, preview, culture);
            return cache.GetCacheItem<ContentRoutingResult>(key, () => GetByRouteInternal(preview, route, hideTopLevelNode, culture));
        }

        public ContentRoutingResult GetByRouteInternal(bool preview, string route, bool? hideTopLevelNode, string culture)
        {
            hideTopLevelNode = hideTopLevelNode ?? _globalSettings.HideTopLevelNodeFromPath; // default = settings

            // the route always needs to be lower case because we only store the urlName attribute in lower case
            route = route.ToLowerInvariant();

            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var startNodeId = pos == 0 ? 0 : int.Parse(route.Substring(0, pos));
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            IPublishedContent content;

            if (startNodeId > 0)
            {
                // if in a domain then start with the root node of the domain
                // and follow the path
                // note: if domain has a path (eg example.com/en) which is not recommended anymore
                //  then /en part of the domain is basically ignored here...
                content = _navigableData.GetById(preview, startNodeId);
                content = FollowRoute(content, parts, 0, culture);
            }
            else if (parts.Length == 0)
            {
                // if not in a domain, and path is empty - what is the default page?
                // let's say it is the first one in the tree, if any -- order by sortOrder
                content = _navigableData.GetAtRoot(preview).FirstOrDefault();
            }
            else
            {
                // if not in a domain...
                // hideTopLevelNode = support legacy stuff, look for /*/path/to/node
                // else normal, look for /path/to/node
                content = hideTopLevelNode.Value
                    ? _navigableData.GetAtRoot(preview).SelectMany(x => x.Children(culture)).FirstOrDefault(x => x.UrlSegment(culture) == parts[0])
                    : _navigableData.GetAtRoot(preview).FirstOrDefault(x => x.UrlSegment(culture) == parts[0]);
                content = FollowRoute(content, parts, 1, culture);
            }

            // if hideTopLevelNodePath is true then for url /foo we looked for /*/foo
            // but maybe that was the url of a non-default top-level node, so we also
            // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
            if (content == null && hideTopLevelNode.Value && parts.Length == 1)
            {
                content = _navigableData.GetAtRoot(preview).FirstOrDefault(x => x.UrlSegment(culture) == parts[0]);
            }
            ContentRoutingResult result = new ContentRoutingResult
            {
                Id = content?.Id ?? 0,
                Outcome = content != null ? RoutingOutcome.Found : RoutingOutcome.NotFound
            };
            return result;
        }
        private IPublishedContent FollowRoute(IPublishedContent content, IReadOnlyList<string> parts, int start, string culture)
        {
            var i = start;
            while (content != null && i < parts.Count)
            {
                var part = parts[i++];
                content = content.Children(culture).FirstOrDefault(x =>
                {
                    var urlSegment = x.UrlSegment(culture);
                    return urlSegment == part;
                });
            }
            return content;
        }


        public string GetRouteById(int contentId, string culture = null)
        {
            return GetRouteById(_previewDefault, contentId, culture);
        }

        public string GetRouteById(bool preview, int contentId, string culture = null)
        {
            var cache = (preview == false || PublishedSnapshotService.FullCacheWhenPreviewing) ? _elementsCache : _snapshotCache;
            var key = CacheKeys.ContentCacheRouteByContent(contentId, preview, culture);
            return cache.GetCacheItem<string>(key, () => GetRouteByIdInternal(preview, contentId, null, culture));
        }

        private string GetRouteByIdInternal(bool preview, int contentId, bool? hideTopLevelNode, string culture)
        {
            var node = _navigableData.GetById(preview, contentId);
            if (node == null)
                return null;

            hideTopLevelNode = hideTopLevelNode ?? _globalSettings.HideTopLevelNodeFromPath; // default = settings

            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting urls in the way
            var pathParts = new List<string>();
            var n = node;
            var urlSegment = n.UrlSegment(culture);
            var hasDomains = _domainCache.HasAssigned(n.Id);
            while (hasDomains == false && n != null) // n is null at root
            {
                // no segment indicates this is not published when this is a variant
                if (urlSegment.IsNullOrWhiteSpace()) return null;

                pathParts.Add(urlSegment);

                // move to parent node
                n = n.Parent;
                if (n != null)
                    urlSegment = n.UrlSegment(culture);

                hasDomains = n != null && _domainCache.HasAssigned(n.Id);
            }

            // at this point this will be the urlSegment of the root, no segment indicates this is not published when this is a variant
            if (urlSegment.IsNullOrWhiteSpace()) return null;

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (hasDomains == false && hideTopLevelNode.Value)
                ApplyHideTopLevelNodeFromPath(node, pathParts, preview);

            // assemble the route
            pathParts.Reverse();
            var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
            //prefix the root node id containing the domain if it exists (this is a standard way of creating route paths)
            //and is done so that we know the ID of the domain node for the path
            var route = (n?.Id.ToString(CultureInfo.InvariantCulture) ?? "") + path;

            return route;
        }

        private void ApplyHideTopLevelNodeFromPath(IPublishedContent content, IList<string> segments, bool preview)
        {
            // in theory if hideTopLevelNodeFromPath is true, then there should be only one
            // top-level node, or else domains should be assigned. but for backward compatibility
            // we add this check - we look for the document matching "/" and if it's not us, then
            // we do not hide the top level path
            // it has to be taken care of in GetByRoute too so if
            // "/foo" fails (looking for "/*/foo") we try also "/foo".
            // this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
            // that's the way it works pre-4.10 and we try to be backward compat for the time being
            if (content.Parent == null)
            {
                var rootNode = _navigableData.GetById(preview, GetIdByRoute(preview, "/", true).Id);
                if (rootNode == null)
                    throw new Exception("Failed to get node at /.");
                if (rootNode.Id == content.Id) // remove only if we're the default node
                    segments.RemoveAt(segments.Count - 1);
            }
            else
            {
                segments.RemoveAt(segments.Count - 1);
            }
        }
    }
}
