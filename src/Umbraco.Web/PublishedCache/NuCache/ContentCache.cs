using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PublishedCache.NuCache
{
    class ContentCache : PublishedCacheBase, IPublishedContentCache, INavigableData, IDisposable
    {
        private readonly ContentStore2.Snapshot _snapshot;
        private readonly ICacheProvider _facadeCache;
        private readonly ICacheProvider _snapshotCache;
        private readonly DomainHelper _domainHelper;

        #region Constructor

        public ContentCache(bool previewDefault, ContentStore2.Snapshot snapshot, ICacheProvider facadeCache, ICacheProvider snapshotCache, DomainHelper domainHelper)
            : base(previewDefault)
        {
            _snapshot = snapshot;
            _facadeCache = facadeCache;
            _snapshotCache = snapshotCache;
            _domainHelper = domainHelper;
        }

        #endregion

        #region Routes

        // routes can be
        // "/"
        // "123/"
        // "/path/to/node"
        // "123/path/to/node"

        // at the moment we try our best to be backward compatible, but really,
        // should get rid of hideTopLevelNode and other oddities entirely, eventually

        public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null)
        {
            return GetByRoute(PreviewDefault, route, hideTopLevelNode);
        }

        public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null)
        {
            if (route == null) throw new ArgumentNullException("route");

            var cache = (preview == false || FacadeService.FullCacheWhenPreviewing) ? _snapshotCache : _facadeCache;
            var key = CacheKeys.ContentCacheContentByRoute(route, preview);
            return cache.GetCacheItem<IPublishedContent>(key, () => GetByRouteInternal(preview, route, hideTopLevelNode));
        }

        private IPublishedContent GetByRouteInternal(bool preview, string route, bool? hideTopLevelNode)
        {
            hideTopLevelNode = hideTopLevelNode ?? GlobalSettings.HideTopLevelNodeFromPath; // default = settings

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
                //  then then /en part of the domain is basically ignored here...
                content = GetById(preview, startNodeId);
                content = FollowRoute(content, parts, 0);
            }
            else if (parts.Length == 0)
            {
                // if not in a domain, and path is empty - what is the default page?
                // let's say it is the first one in the tree, if any -- order by sortOrder
                content = GetAtRoot(preview).FirstOrDefault();
            }
            else
            {
                // if not in a domain...
                // hideTopLevelNode = support legacy stuff, look for /*/path/to/node
                // else normal, look for /path/to/node
                content = hideTopLevelNode.Value
                    ? GetAtRoot(preview).SelectMany(x => x.Children).FirstOrDefault(x => x.UrlName == parts[0])
                    : GetAtRoot(preview).FirstOrDefault(x => x.UrlName == parts[0]);
                content = FollowRoute(content, parts, 1);
            }

            // if hideTopLevelNodePath is true then for url /foo we looked for /*/foo
            // but maybe that was the url of a non-default top-level node, so we also
            // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
            if (content == null && hideTopLevelNode.Value && parts.Length == 1)
            {
                content = GetAtRoot(preview).FirstOrDefault(x => x.UrlName == parts[0]);
            }

            return content;
        }

        public string GetRouteById(int contentId)
        {
            return GetRouteById(PreviewDefault, contentId);
        }

        public string GetRouteById(bool preview, int contentId)
        {
            var cache = (preview == false || FacadeService.FullCacheWhenPreviewing) ? _snapshotCache : _facadeCache;
            var key = CacheKeys.ContentCacheRouteByContent(contentId, preview);
            return cache.GetCacheItem<string>(key, () => GetRouteByIdInternal(preview, contentId, null));
        }

        private string GetRouteByIdInternal(bool preview, int contentId, bool? hideTopLevelNode)
        {
            var node = GetById(preview, contentId);
            if (node == null)
                return null;

            hideTopLevelNode = hideTopLevelNode ?? GlobalSettings.HideTopLevelNodeFromPath; // default = settings

            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting urls in the way
            var pathParts = new List<string>();
            var n = node;
            var hasDomains = _domainHelper.NodeHasDomains(n.Id);
            while (hasDomains == false && n != null) // n is null at root
            {
                // get the url
                var urlName = n.UrlName;
                pathParts.Add(urlName);

                // move to parent node
                n = n.Parent;
                hasDomains = n != null && _domainHelper.NodeHasDomains(n.Id);
            }

            // no domain, respect HideTopLevelNodeFromPath for legacy purposes
            if (hasDomains == false && hideTopLevelNode.Value)
                ApplyHideTopLevelNodeFromPath(node, pathParts, preview);

            // assemble the route
            pathParts.Reverse();
            var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
            var route = (n == null ? "" : n.Id.ToString(CultureInfo.InvariantCulture)) + path;

            return route;
        }

        private static IPublishedContent FollowRoute(IPublishedContent content, IReadOnlyList<string> parts, int start)
        {
            var i = start;
            while (content != null && i < parts.Count)
            {
                var part = parts[i++];
                content = content.Children.FirstOrDefault(x => x.UrlName == part);
            }
            return content;
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
                var rootNode = GetByRoute(preview, "/", true);
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

        #endregion

        #region Get, Has

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            if (n == null) return null;

            // both .Draft and .Published cannot be null at the same time
            return preview
                ? n.Draft ?? GetPublishedContentAsPreviewing(n.Published)
                : n.Published;
        }

        public override bool HasById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            if (n == null) return false;

            return preview || n.Published != null;
        }

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview)
        {
            if (FacadeService.CacheContentCacheRoots == false)
                return GetAtRootNoCache(preview);

            var facade = Facade.Current;
            var cache = (facade == null)
                ? null
                : (preview == false || FacadeService.FullCacheWhenPreviewing
                    ? facade.SnapshotCache
                    : facade.FacadeCache);

            if (cache == null)
                return GetAtRootNoCache(preview);

            // note: ToArray is important here, we want to cache the result, not the function!
            return (IEnumerable<IPublishedContent>) cache.GetCacheItem(
                CacheKeys.ContentCacheRoots(preview),
                () => GetAtRootNoCache(preview).ToArray());
        }

        private IEnumerable<IPublishedContent> GetAtRootNoCache(bool preview)
        {
            var c = _snapshot.GetAtRoot();

            // both .Draft and .Published cannot be null at the same time
            return c.Select(n => preview
                ? n.Draft ?? GetPublishedContentAsPreviewing(n.Published)
                : n.Published).WhereNotNull().OrderBy(x => x.SortOrder);
        }

        // gets a published content as a previewing draft, if preview is true
        // this is for published content when previewing
        internal static IPublishedContent GetPublishedContentAsPreviewing(IPublishedContent content /*, bool preview*/)
        {
            if (content == null /*|| preview == false*/) return null; //content;
            
            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.

            var inner = PublishedContent.UnwrapIPublishedContent(content);
            return inner.AsPreviewingModel();
        }

        public override bool HasContent(bool preview)
        {
            return preview
                ? _snapshot.IsEmpty == false
                : _snapshot.GetAtRoot().Any(x => x.Published != null);
        }

        #endregion

        #region XPath

        public override IPublishedContent GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetSingleByXPath(iterator);
        }

        public override IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetSingleByXPath(iterator);
        }

        private static IPublishedContent GetSingleByXPath(XPathNodeIterator iterator)
        {
            if (iterator.MoveNext() == false) return null;

            var xnav = iterator.Current as NavigableNavigator;
            if (xnav == null) return null;

            var xcontent = xnav.UnderlyingObject as NavigableContent;
            return xcontent == null ? null : xcontent.InnerContent;
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetByXPath(iterator);
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetByXPath(iterator);
        }

        private static IEnumerable<IPublishedContent> GetByXPath(XPathNodeIterator iterator)
        {
            while (iterator.MoveNext())
            {
                var xnav = iterator.Current as NavigableNavigator;
                if (xnav == null) continue;

                var xcontent = xnav.UnderlyingObject as NavigableContent;
                if (xcontent == null) continue;

                yield return xcontent.InnerContent;
            }
        }

        public override XPathNavigator CreateNavigator(bool preview)
        {
            var source = new Source(this, preview);
            var navigator = new NavigableNavigator(source);
            return navigator;
        }

        public override XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            var source = new Source(this, preview);
            var navigator = new NavigableNavigator(source);
            return navigator.CloneWithNewRoot(id, 0);
        }

        #endregion

        #region Detached

        // detached is something that needs to be refactored entirely eventually
        // detached property should accept the "container content" guid
        // etc

        public IPublishedProperty CreateDetachedProperty(PublishedPropertyType propertyType, object value, bool isPreviewing)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Content types

        public override PublishedContentType GetContentType(int id)
        {
            return _snapshot.GetContentType(id);
        }

        public override PublishedContentType GetContentType(string alias)
        {
            return _snapshot.GetContentType(alias);
        }

        public override IEnumerable<IPublishedContent> GetByContentType(PublishedContentType contentType)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _snapshot.Dispose();
        }

        #endregion
    }
}
