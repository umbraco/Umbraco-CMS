using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class ContentCache : PublishedCacheBase, IPublishedContentCache2, INavigableData, IDisposable
    {
        private readonly ContentStore.Snapshot _snapshot;
        private readonly IAppCache _snapshotCache;
        private readonly IAppCache _elementsCache;
        private readonly IDomainCache _domainCache;
        private readonly IGlobalSettings _globalSettings;
        private readonly IVariationContextAccessor _variationContextAccessor;

        #region Constructor

        // TODO: figure this out
        // after the current snapshot has been resync-ed
        // it's too late for UmbracoContext which has captured previewDefault and stuff into these ctor vars
        // but, no, UmbracoContext returns snapshot.Content which comes from elements SO a resync should create a new cache

        public ContentCache(bool previewDefault, ContentStore.Snapshot snapshot, IAppCache snapshotCache, IAppCache elementsCache, IDomainCache domainCache, IGlobalSettings globalSettings, IVariationContextAccessor variationContextAccessor)
            : base(previewDefault)
        {
            _snapshot = snapshot;
            _snapshotCache = snapshotCache;
            _elementsCache = elementsCache;
            _domainCache = domainCache;
            _globalSettings = globalSettings;
            _variationContextAccessor = variationContextAccessor;
        }

        private bool HideTopLevelNodeFromPath => _globalSettings.HideTopLevelNodeFromPath;

        #endregion

        #region Routes

        // routes can be
        // "/"
        // "123/"
        // "/path/to/node"
        // "123/path/to/node"

        // at the moment we try our best to be backward compatible, but really,
        // should get rid of hideTopLevelNode and other oddities entirely, eventually

        public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null, string culture = null)
        {
            return GetByRoute(PreviewDefault, route, hideTopLevelNode, culture);
        }

        public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string culture = null)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            var cache = preview == false || PublishedSnapshotService.FullCacheWhenPreviewing ? _elementsCache : _snapshotCache;
            var key = CacheKeys.ContentCacheContentByRoute(route, preview, culture);
            return cache.GetCacheItem<IPublishedContent>(key, () => GetByRouteInternal(preview, route, hideTopLevelNode, culture));
        }

        private IPublishedContent GetByRouteInternal(bool preview, string route, bool? hideTopLevelNode, string culture)
        {
            hideTopLevelNode = hideTopLevelNode ?? HideTopLevelNodeFromPath; // default = settings

            // the route always needs to be lower case because we only store the urlName attribute in lower case
            route = route.ToLowerInvariant();

            var pos = route.IndexOf('/');
            var path = pos == 0 ? route : route.Substring(pos);
            var startNodeId = pos == 0 ? 0 : int.Parse(route.Substring(0, pos));
            var parts = path.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);

            IPublishedContent content;

            if (startNodeId > 0)
            {
                // if in a domain then start with the root node of the domain
                // and follow the path
                // note: if domain has a path (eg example.com/en) which is not recommended anymore
                //  then /en part of the domain is basically ignored here...
                content = GetById(preview, startNodeId);
                content = FollowRoute(content, parts, 0, culture);
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
                    ? GetAtRoot(preview).SelectMany(x => x.Children(culture)).FirstOrDefault(x => x.UrlSegment(culture) == parts[0])
                    : GetAtRoot(preview).FirstOrDefault(x => x.UrlSegment(culture) == parts[0]);
                content = FollowRoute(content, parts, 1, culture);
            }

            // if hideTopLevelNodePath is true then for URL /foo we looked for /*/foo
            // but maybe that was the URL of a non-default top-level node, so we also
            // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
            if (content == null && hideTopLevelNode.Value && parts.Length == 1)
            {
                content = GetAtRoot(preview).FirstOrDefault(x => x.UrlSegment(culture) == parts[0]);
            }

            return content;
        }

        public string GetRouteById(int contentId, string culture = null)
        {
            return GetRouteById(PreviewDefault, contentId, culture);
        }

        public string GetRouteById(bool preview, int contentId, string culture = null)
        {
            var cache = (preview == false || PublishedSnapshotService.FullCacheWhenPreviewing) ? _elementsCache : _snapshotCache;
            var key = CacheKeys.ContentCacheRouteByContent(contentId, preview, culture);
            return cache.GetCacheItem<string>(key, () => GetRouteByIdInternal(preview, contentId, null, culture));
        }

        private string GetRouteByIdInternal(bool preview, int contentId, bool? hideTopLevelNode, string culture)
        {
            var node = GetById(preview, contentId);
            if (node == null)
                return null;

            hideTopLevelNode = hideTopLevelNode ?? HideTopLevelNodeFromPath; // default = settings

            // walk up from that node until we hit a node with a domain,
            // or we reach the content root, collecting URLs in the way
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
            var node = _snapshot.Get(contentId);
            return GetNodePublishedContent(node, preview);
        }

        public override IPublishedContent GetById(bool preview, Guid contentId)
        {
            var node = _snapshot.Get(contentId);
            return GetNodePublishedContent(node, preview);
        }

        public override IPublishedContent GetById(bool preview, Udi contentId)
        {
            var guidUdi = contentId as GuidUdi;
            if (guidUdi == null)
                throw new ArgumentException($"Udi must be of type {typeof(GuidUdi).Name}.", nameof(contentId));

            if (guidUdi.EntityType != Constants.UdiEntityType.Document)
                throw new ArgumentException($"Udi entity type must be \"{Constants.UdiEntityType.Document}\".", nameof(contentId));

            return GetById(preview, guidUdi.Guid);
        }

        public override bool HasById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            if (n == null) return false;

            return preview || n.PublishedModel != null;
        }

        IEnumerable<IPublishedContent> INavigableData.GetAtRoot(bool preview) => GetAtRoot(preview);

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
        {
            // handle context culture for variant
            if (culture == null)
                culture = _variationContextAccessor?.VariationContext?.Culture ?? "";

            // _snapshot.GetAtRoot() returns all ContentNode at root
            // both .Draft and .Published cannot be null at the same time
            // root is already sorted by sortOrder, and does not contain nulls
            //
            // GetNodePublishedContent may return null if !preview and there is no
            // published model, so we need to filter these nulls out

            var atRoot = _snapshot.GetAtRoot()
                .Select(n => GetNodePublishedContent(n, preview))
                .WhereNotNull();

            // if a culture is specified, we must ensure that it is avail/published
            if (culture != "*")
                atRoot = atRoot.Where(x => x.IsInvariantOrHasCulture(culture));

            return atRoot;
        }

        private static IPublishedContent GetNodePublishedContent(ContentNode node, bool preview)
        {
            if (node == null)
                return null;

            // both .Draft and .Published cannot be null at the same time

            return preview
                ? node.DraftModel ?? GetPublishedContentAsDraft(node.PublishedModel)
                : node.PublishedModel;
        }

        // gets a published content as a previewing draft, if preview is true
        // this is for published content when previewing
        private static IPublishedContent GetPublishedContentAsDraft(IPublishedContent content /*, bool preview*/)
        {
            if (content == null /*|| preview == false*/) return null; //content;

            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.

            var inner = PublishedContent.UnwrapIPublishedContent(content);
            return inner.AsDraft();
        }

        public override bool HasContent(bool preview)
        {
            return preview
                ? _snapshot.IsEmpty == false
                : _snapshot.GetAtRoot().Any(x => x.PublishedModel != null);
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
            var xcontent = xnav?.UnderlyingObject as NavigableContent;
            return xcontent?.InnerContent;
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
            iterator = iterator.Clone();
            while (iterator.MoveNext())
            {
                var xnav = iterator.Current as NavigableNavigator;
                var xcontent = xnav?.UnderlyingObject as NavigableContent;
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

        #region Content types

        public override IPublishedContentType GetContentType(int id) => _snapshot.GetContentType(id);

        public override IPublishedContentType GetContentType(string alias) => _snapshot.GetContentType(alias);

        public override IPublishedContentType GetContentType(Guid key) => _snapshot.GetContentType(key);

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _snapshot.Dispose();
        }

        #endregion
    }
}
