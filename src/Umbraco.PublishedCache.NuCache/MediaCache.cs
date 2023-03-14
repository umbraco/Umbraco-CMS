using System.Xml.XPath;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Xml;
using Umbraco.Cms.Core.Xml.XPath;
using Umbraco.Cms.Infrastructure.PublishedCache.Navigable;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

public class MediaCache : PublishedCacheBase, IPublishedMediaCache, INavigableData, IDisposable
{
    private readonly ContentStore.Snapshot _snapshot;
    private readonly IVariationContextAccessor _variationContextAccessor;

    #region Constructors

    public MediaCache(bool previewDefault, ContentStore.Snapshot snapshot, IVariationContextAccessor variationContextAccessor)
        : base(previewDefault)
    {
        _snapshot = snapshot;
        _variationContextAccessor = variationContextAccessor;
    }

    #endregion

    #region IDisposable

    public void Dispose() => _snapshot.Dispose();

    #endregion

    #region Get, Has

    public override IPublishedContent? GetById(bool preview, int contentId)
    {
        // ignore preview, there's only draft for media
        ContentNode? n = _snapshot.Get(contentId);
        return n?.PublishedModel;
    }

    public override IPublishedContent? GetById(bool preview, Guid contentId)
    {
        // ignore preview, there's only draft for media
        ContentNode? n = _snapshot.Get(contentId);
        return n?.PublishedModel;
    }

    public override IPublishedContent? GetById(bool preview, Udi contentId)
    {
        var guidUdi = contentId as GuidUdi;
        if (guidUdi == null)
        {
            throw new ArgumentException($"Udi must be of type {typeof(GuidUdi).Name}.", nameof(contentId));
        }

        if (guidUdi.EntityType != Constants.UdiEntityType.Media)
        {
            throw new ArgumentException(
                $"Udi entity type must be \"{Constants.UdiEntityType.Media}\".",
                nameof(contentId));
        }

        // ignore preview, there's only draft for media
        ContentNode? n = _snapshot.Get(guidUdi.Guid);
        return n?.PublishedModel;
    }

    public override bool HasById(bool preview, int contentId)
    {
        ContentNode? n = _snapshot.Get(contentId);
        return n != null;
    }

    IEnumerable<IPublishedContent> INavigableData.GetAtRoot(bool preview) => GetAtRoot(preview);

    public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        // handle context culture for variant
        if (culture == null)
        {
            culture = _variationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        IEnumerable<IPublishedContent?> atRoot = _snapshot.GetAtRoot().Select(x => x.PublishedModel);
        return culture == "*"
            ? atRoot.WhereNotNull()
            : atRoot.Where(x => x?.IsInvariantOrHasCulture(culture) ?? false).WhereNotNull();
    }

    public override bool HasContent(bool preview) => _snapshot.IsEmpty == false;

    #endregion

    #region XPath

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

    public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars)
    {
        XPathNavigator navigator = CreateNavigator(preview);
        XPathNodeIterator iterator = navigator.Select(xpath, vars);
        return GetByXPath(iterator);
    }

    private static IPublishedContent? GetSingleByXPath(XPathNodeIterator iterator)
    {
        if (iterator.MoveNext() == false)
        {
            return null;
        }

        if (iterator.Current is not NavigableNavigator xnav)
        {
            return null;
        }

        var xcontent = xnav.UnderlyingObject as NavigableContent;
        return xcontent?.InnerContent;
    }

    public override IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
    {
        XPathNavigator navigator = CreateNavigator(preview);
        XPathNodeIterator iterator = navigator.Select(xpath, vars);
        return GetByXPath(iterator);
    }

    public override XPathNavigator CreateNavigator(bool preview)
    {
        var source = new Source(this, preview);
        var navigator = new NavigableNavigator(source);
        return navigator;
    }

    private static IEnumerable<IPublishedContent> GetByXPath(XPathNodeIterator iterator)
    {
        while (iterator.MoveNext())
        {
            if (iterator.Current is not NavigableNavigator xnav)
            {
                continue;
            }

            if (xnav.UnderlyingObject is not NavigableContent xcontent)
            {
                continue;
            }

            yield return xcontent.InnerContent;
        }
    }

    public override XPathNavigator? CreateNodeNavigator(int id, bool preview)
    {
        var source = new Source(this, preview);
        var navigator = new NavigableNavigator(source);
        return navigator.CloneWithNewRoot(id, 0);
    }

    #endregion

    #region Content types

    public override IPublishedContentType? GetContentType(int id) => _snapshot.GetContentType(id);

    public override IPublishedContentType? GetContentType(string alias) => _snapshot.GetContentType(alias);

    public override IPublishedContentType? GetContentType(Guid key) => _snapshot.GetContentType(key);

    #endregion
}
