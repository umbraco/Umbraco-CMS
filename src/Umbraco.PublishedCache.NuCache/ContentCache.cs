using System.Globalization;
using System.Xml.XPath;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Xml;
using Umbraco.Cms.Core.Xml.XPath;
using Umbraco.Cms.Infrastructure.PublishedCache.Navigable;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

public class ContentCache : PublishedCacheBase, IPublishedContentCache, INavigableData, IDisposable
{
    private readonly IDomainCache _domainCache;
    private readonly IAppCache? _elementsCache;
    private readonly GlobalSettings _globalSettings;
    private readonly ContentStore.Snapshot _snapshot;
    private readonly IAppCache _snapshotCache;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public void Dispose() => _snapshot.Dispose();

    // TODO: figure this out
    // after the current snapshot has been resync-ed
    // it's too late for UmbracoContext which has captured previewDefault and stuff into these ctor vars
    // but, no, UmbracoContext returns snapshot.Content which comes from elements SO a resync should create a new cache
    public ContentCache(
        bool previewDefault,
        ContentStore.Snapshot snapshot,
        IAppCache snapshotCache,
        IAppCache? elementsCache,
        IDomainCache domainCache,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor variationContextAccessor)
        : base(previewDefault)
    {
        _snapshot = snapshot;
        _snapshotCache = snapshotCache;
        _elementsCache = elementsCache;
        _domainCache = domainCache ?? throw new ArgumentNullException(nameof(domainCache));
        _globalSettings = globalSettings.Value;
        _variationContextAccessor = variationContextAccessor;
    }

    private bool HideTopLevelNodeFromPath => _globalSettings.HideTopLevelNodeFromPath;

    // routes can be
    // "/"
    // "123/"
    // "/path/to/node"
    // "123/path/to/node"

    // at the moment we try our best to be backward compatible, but really,
    // should get rid of hideTopLevelNode and other oddities entirely, eventually
    public IPublishedContent? GetByRoute(string route, bool? hideTopLevelNode = null, string? culture = null) =>
        GetByRoute(PreviewDefault, route, hideTopLevelNode, culture);

    public IPublishedContent? GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string? culture = null)
    {
        if (route == null)
        {
            throw new ArgumentNullException(nameof(route));
        }

        IAppCache? cache = preview == false || PublishedSnapshotService.FullCacheWhenPreviewing
            ? _elementsCache
            : _snapshotCache;
        var key = CacheKeys.ContentCacheContentByRoute(route, preview, culture);
        return cache?.GetCacheItem(key, () => GetByRouteInternal(preview, route, hideTopLevelNode, culture));
    }

    private IPublishedContent? GetByRouteInternal(bool preview, string route, bool? hideTopLevelNode, string? culture)
    {
        hideTopLevelNode = hideTopLevelNode ?? HideTopLevelNodeFromPath; // default = settings

        // the route always needs to be lower case because we only store the urlName attribute in lower case
        route = route.ToLowerInvariant();

        var pos = route.IndexOf('/');
        var path = pos == 0 ? route : route.Substring(pos);
        var startNodeId = pos == 0 ? 0 : int.Parse(route.Substring(0, pos), CultureInfo.InvariantCulture);
        var parts = path.Split(Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);

        IPublishedContent? content;

        if ((!_globalSettings.ForceCombineUrlPathLeftToRight
             && CultureInfo.GetCultureInfo(culture ?? _globalSettings.DefaultUILanguage).TextInfo.IsRightToLeft))
        {
            parts = parts.Reverse().ToArray();
        }

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
                ? GetAtRoot(preview)
                    .SelectMany(x => x.Children(_variationContextAccessor, culture)!) // Do we suppress here?
                    .FirstOrDefault(x => x.UrlSegment(_variationContextAccessor, culture) == parts[0])
                : GetAtRoot(preview)
                    .FirstOrDefault(x => x.UrlSegment(_variationContextAccessor, culture) == parts[0]);
            content = FollowRoute(content, parts, 1, culture);
        }

        // if hideTopLevelNodePath is true then for URL /foo we looked for /*/foo
        // but maybe that was the URL of a non-default top-level node, so we also
        // have to look for /foo (see note in ApplyHideTopLevelNodeFromPath).
        if (content == null && hideTopLevelNode.Value && parts.Length == 1)
        {
            content = GetAtRoot(preview)
                .FirstOrDefault(x => x.UrlSegment(_variationContextAccessor, culture) == parts[0]);
        }

        return content;
    }

    public string? GetRouteById(int contentId, string? culture = null) =>
        GetRouteById(PreviewDefault, contentId, culture);

    public string? GetRouteById(bool preview, int contentId, string? culture = null)
    {
        IAppCache? cache = preview == false || PublishedSnapshotService.FullCacheWhenPreviewing
            ? _elementsCache
            : _snapshotCache;
        var key = CacheKeys.ContentCacheRouteByContent(contentId, preview, culture);
        return cache?.GetCacheItem(key, () => GetRouteByIdInternal(preview, contentId, null, culture));
    }

    private string? GetRouteByIdInternal(bool preview, int contentId, bool? hideTopLevelNode, string? culture)
    {
        IPublishedContent? node = GetById(preview, contentId);
        if (node == null)
        {
            return null;
        }

        hideTopLevelNode = hideTopLevelNode ?? HideTopLevelNodeFromPath; // default = settings

        // walk up from that node until we hit a node with a domain,
        // or we reach the content root, collecting URLs in the way
        var pathParts = new List<string>();
        IPublishedContent? content = node;
        var urlSegment = content.UrlSegment(_variationContextAccessor, culture);
        var hasDomains = _domainCache.GetAssignedWithCulture(culture, content.Id);

        // content is null at root
        while (hasDomains == false && content != null)
        {
            // no segment indicates this is not published when this is a variant
            if (urlSegment.IsNullOrWhiteSpace())
            {
                return null;
            }

            pathParts.Add(urlSegment!);

            // move to parent node
            content = content.Parent;
            if (content != null)
            {
                urlSegment = content.UrlSegment(_variationContextAccessor, culture);
            }

            hasDomains = content != null && _domainCache.GetAssignedWithCulture(culture, content.Id);
        }

        // at this point this will be the urlSegment of the root, no segment indicates this is not published when this is a variant
        if (urlSegment.IsNullOrWhiteSpace())
        {
            return null;
        }

        // no domain, respect HideTopLevelNodeFromPath for legacy purposes
        if (hasDomains == false && hideTopLevelNode.Value)
        {
            ApplyHideTopLevelNodeFromPath(node, pathParts, preview);
        }

        // assemble the route- We only have to reverse for left to right languages
        if ((_globalSettings.ForceCombineUrlPathLeftToRight
             || !CultureInfo.GetCultureInfo(culture ?? _globalSettings.DefaultUILanguage).TextInfo.IsRightToLeft))
        {
            pathParts.Reverse();
        }

        var path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc

        // prefix the root node id containing the domain if it exists (this is a standard way of creating route paths)
        // and is done so that we know the ID of the domain node for the path
        var route = (content?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty) + path;

        return route;
    }

    private IPublishedContent? FollowRoute(IPublishedContent? content, IReadOnlyList<string> parts, int start, string? culture)
    {
        var i = start;
        while (content != null && i < parts.Count)
        {
            var part = parts[i++];
            content = content.Children(_variationContextAccessor, culture)?.FirstOrDefault(x =>
            {
                var urlSegment = x.UrlSegment(_variationContextAccessor, culture);
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
            IPublishedContent? rootNode = GetByRoute(preview, "/", true);
            if (rootNode == null)
            {
                throw new Exception("Failed to get node at /. This might be because you're trying to publish a variant, with no domains setup");
            }

            // remove only if we're the default node
            if (rootNode.Id == content.Id)
            {
                segments.RemoveAt(segments.Count - 1);
            }
        }
        else
        {
            segments.RemoveAt(segments.Count - 1);
        }
    }

    public override IPublishedContent? GetById(bool preview, int contentId)
    {
        ContentNode? node = _snapshot.Get(contentId);
        return GetNodePublishedContent(node, preview);
    }

    public override IPublishedContent? GetById(bool preview, Guid contentId)
    {
        ContentNode? node = _snapshot.Get(contentId);
        return GetNodePublishedContent(node, preview);
    }

    public override IPublishedContent? GetById(bool preview, Udi contentId)
    {
        var guidUdi = contentId as GuidUdi;
        if (guidUdi == null)
        {
            throw new ArgumentException($"Udi must be of type {typeof(GuidUdi).Name}.", nameof(contentId));
        }

        if (guidUdi.EntityType != Constants.UdiEntityType.Document)
        {
            throw new ArgumentException($"Udi entity type must be \"{Constants.UdiEntityType.Document}\".", nameof(contentId));
        }

        return GetById(preview, guidUdi.Guid);
    }

    public override bool HasById(bool preview, int contentId)
    {
        ContentNode? n = _snapshot.Get(contentId);
        if (n == null)
        {
            return false;
        }

        return preview || n.PublishedModel != null;
    }

    IEnumerable<IPublishedContent> INavigableData.GetAtRoot(bool preview) => GetAtRoot(preview);

    public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        // handle context culture for variant
        if (culture == null)
        {
            culture = _variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // _snapshot.GetAtRoot() returns all ContentNode at root
        // both .Draft and .Published cannot be null at the same time
        // root is already sorted by sortOrder, and does not contain nulls
        //
        // GetNodePublishedContent may return null if !preview and there is no
        // published model, so we need to filter these nulls out
        IEnumerable<IPublishedContent> atRoot = _snapshot.GetAtRoot()
            .Select(n => GetNodePublishedContent(n, preview))
            .WhereNotNull();

        // if a culture is specified, we must ensure that it is avail/published
        if (culture != "*")
        {
            atRoot = atRoot.Where(x => x.IsInvariantOrHasCulture(culture));
        }

        return atRoot;
    }

    private static IPublishedContent? GetNodePublishedContent(ContentNode? node, bool preview)
    {
        if (node == null)
        {
            return null;
        }

        // both .Draft and .Published cannot be null at the same time
        return preview
            ? node.DraftModel ?? GetPublishedContentAsDraft(node.PublishedModel)
            : node.PublishedModel;
    }

    // gets a published content as a previewing draft, if preview is true
    // this is for published content when previewing
    private static IPublishedContent? GetPublishedContentAsDraft(IPublishedContent? content /*, bool preview*/)
    {
        if (content == null /*|| preview == false*/)
        {
            return null; // content;
        }

        // an object in the cache is either an IPublishedContentOrMedia,
        // or a model inheriting from PublishedContentExtended - in which
        // case we need to unwrap to get to the original IPublishedContentOrMedia.
        var inner = PublishedContent.UnwrapIPublishedContent(content);
        return inner.AsDraft();
    }

    public override bool HasContent(bool preview) =>
        preview
            ? _snapshot.IsEmpty == false
            : _snapshot.GetAtRoot().Any(x => x.PublishedModel != null);

    public override IPublishedContent? GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars)
    {
        XPathNavigator navigator = CreateNavigator(preview);
        XPathNodeIterator iterator = navigator.Select(xpath, vars);
        return GetSingleByXPath(iterator);
    }

    public override IPublishedContent? GetSingleByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
    {
        XPathNavigator navigator = CreateNavigator(preview);
        XPathNodeIterator iterator = navigator.Select(xpath, vars);
        return GetSingleByXPath(iterator);
    }

    private static IPublishedContent? GetSingleByXPath(XPathNodeIterator iterator)
    {
        if (iterator.MoveNext() == false)
        {
            return null;
        }

        var xnav = iterator.Current as NavigableNavigator;
        var xcontent = xnav?.UnderlyingObject as NavigableContent;
        return xcontent?.InnerContent;
    }

    public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars)
    {
        XPathNavigator navigator = CreateNavigator(preview);
        XPathNodeIterator iterator = navigator.Select(xpath, vars);
        return GetByXPath(iterator);
    }

    public override IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
    {
        XPathNavigator navigator = CreateNavigator(preview);
        XPathNodeIterator iterator = navigator.Select(xpath, vars);
        return GetByXPath(iterator);
    }

    private static IEnumerable<IPublishedContent> GetByXPath(XPathNodeIterator iterator)
    {
        iterator = iterator.Clone();
        while (iterator.MoveNext())
        {
            var xnav = iterator.Current as NavigableNavigator;
            var xcontent = xnav?.UnderlyingObject as NavigableContent;
            if (xcontent == null)
            {
                continue;
            }

            yield return xcontent.InnerContent;
        }
    }

    public override XPathNavigator CreateNavigator(bool preview)
    {
        var source = new Source(this, preview);
        var navigator = new NavigableNavigator(source);
        return navigator;
    }

    public override XPathNavigator? CreateNodeNavigator(int id, bool preview)
    {
        var source = new Source(this, preview);
        var navigator = new NavigableNavigator(source);
        return navigator.CloneWithNewRoot(id, 0);
    }

    public override IPublishedContentType? GetContentType(int id) => _snapshot.GetContentType(id);

    public override IPublishedContentType? GetContentType(string alias) => _snapshot.GetContentType(alias);

    public override IPublishedContentType? GetContentType(Guid key) => _snapshot.GetContentType(key);
}
