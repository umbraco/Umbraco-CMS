using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PublishedCache;

internal class PublishedElementPropertyBase : PublishedPropertyBase
{
    protected readonly IPublishedElement Element;

    // define constant - determines whether to use cache when previewing
    // to store eg routes, property converted values, anything - caching
    // means faster execution, but uses memory - not sure if we want it
    // so making it configurable.
    private const bool FullCacheWhenPreviewing = true;
    private readonly object _locko = new();
    private readonly IPublishedSnapshotAccessor? _publishedSnapshotAccessor;
    private readonly object? _sourceValue;
    protected readonly bool IsMember;
    protected readonly bool IsPreviewing;
    private CacheValues? _cacheValues;

    private bool _interInitialized;
    private object? _interValue;
    private string? _valuesCacheKey;

    public PublishedElementPropertyBase(
        IPublishedPropertyType propertyType,
        IPublishedElement element,
        bool previewing,
        PropertyCacheLevel referenceCacheLevel,
        object? sourceValue = null,
        IPublishedSnapshotAccessor? publishedSnapshotAccessor = null)
        : base(propertyType, referenceCacheLevel)
    {
        _sourceValue = sourceValue;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        Element = element;
        IsPreviewing = previewing;
        IsMember = propertyType.ContentType?.ItemType == PublishedItemType.Member;
    }

    // used to cache the CacheValues of this property
    // ReSharper disable InconsistentlySynchronizedField
    internal string ValuesCacheKey => _valuesCacheKey ??= PropertyCacheValues(Element.Key, Alias, IsPreviewing);

    public static string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing) =>
        "PublishedSnapshot.Property.CacheValues[" + (previewing ? "D:" : "P:") + contentUid + ":" + typeAlias + "]";

    // ReSharper restore InconsistentlySynchronizedField
    public override bool HasValue(string? culture = null, string? segment = null)
    {
        var hasValue = PropertyType.IsValue(_sourceValue, PropertyValueLevel.Source);
        if (hasValue.HasValue)
        {
            return hasValue.Value;
        }

        GetCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel);

        lock (_locko)
        {
            var value = GetInterValue();
            hasValue = PropertyType.IsValue(value, PropertyValueLevel.Inter);
            if (hasValue.HasValue)
            {
                return hasValue.Value;
            }

            CacheValues cacheValues = GetCacheValues(cacheLevel);
            if (!cacheValues.ObjectInitialized)
            {
                cacheValues.ObjectValue =
                    PropertyType.ConvertInterToObject(Element, referenceCacheLevel, value, IsPreviewing);
                cacheValues.ObjectInitialized = true;
            }

            value = cacheValues.ObjectValue;
            return PropertyType.IsValue(value, PropertyValueLevel.Object) ?? false;
        }
    }

    public override object? GetSourceValue(string? culture = null, string? segment = null) => _sourceValue;

    private void GetCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel)
    {
        // based upon the current reference cache level (ReferenceCacheLevel) and this property
        // cache level (PropertyType.CacheLevel), determines both the actual cache level for the
        // property, and the new reference cache level.

        // if the property cache level is 'shorter-termed' that the reference
        // then use it and it becomes the new reference, else use Content and
        // don't change the reference.
        //
        // examples:
        // currently (reference) caching at published snapshot, property specifies
        // elements, ok to use element. OTOH, currently caching at elements,
        // property specifies snapshot, need to use snapshot.
        if (PropertyType.CacheLevel > ReferenceCacheLevel || PropertyType.CacheLevel == PropertyCacheLevel.None)
        {
            cacheLevel = PropertyType.CacheLevel;
            referenceCacheLevel = cacheLevel;
        }
        else
        {
            cacheLevel = PropertyCacheLevel.Element;
            referenceCacheLevel = ReferenceCacheLevel;
        }
    }

    private IAppCache? GetSnapshotCache()
    {
        // cache within the snapshot cache, unless previewing, then use the snapshot or
        // elements cache (if we don't want to pollute the elements cache with short-lived
        // data) depending on settings
        // for members, always cache in the snapshot cache - never pollute elements cache
        if (_publishedSnapshotAccessor is null)
        {
            return null;
        }

        if (!_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot))
        {
            return null;
        }

        return (IsPreviewing == false || FullCacheWhenPreviewing) && IsMember == false
            ? publishedSnapshot!.ElementsCache
            : publishedSnapshot!.SnapshotCache;
    }

    private CacheValues GetCacheValues(PropertyCacheLevel cacheLevel)
    {
        CacheValues cacheValues;
        switch (cacheLevel)
        {
            case PropertyCacheLevel.None:
                // never cache anything
                cacheValues = new CacheValues();
                break;
            case PropertyCacheLevel.Element:
                // cache within the property object itself, ie within the content object
                cacheValues = _cacheValues ??= new CacheValues();
                break;
            case PropertyCacheLevel.Elements:
                // cache within the elements  cache, depending...
                IAppCache? snapshotCache = GetSnapshotCache();
                cacheValues = (CacheValues?)snapshotCache?.Get(ValuesCacheKey, () => new CacheValues()) ??
                              new CacheValues();
                break;
            case PropertyCacheLevel.Snapshot:
                IPublishedSnapshot? publishedSnapshot = _publishedSnapshotAccessor?.GetRequiredPublishedSnapshot();

                // cache within the snapshot cache
                IAppCache? facadeCache = publishedSnapshot?.SnapshotCache;
                cacheValues = (CacheValues?)facadeCache?.Get(ValuesCacheKey, () => new CacheValues()) ??
                              new CacheValues();
                break;
            default:
                throw new InvalidOperationException("Invalid cache level.");
        }

        return cacheValues;
    }

    private object? GetInterValue()
    {
        if (_interInitialized)
        {
            return _interValue;
        }

        _interValue = PropertyType.ConvertSourceToInter(Element, _sourceValue, IsPreviewing);
        _interInitialized = true;
        return _interValue;
    }

    public override object? GetValue(string? culture = null, string? segment = null)
    {
        GetCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel);

        lock (_locko)
        {
            CacheValues cacheValues = GetCacheValues(cacheLevel);
            if (cacheValues.ObjectInitialized)
            {
                return cacheValues.ObjectValue;
            }

            cacheValues.ObjectValue =
                PropertyType.ConvertInterToObject(Element, referenceCacheLevel, GetInterValue(), IsPreviewing);
            cacheValues.ObjectInitialized = true;
            return cacheValues.ObjectValue;
        }
    }

    public override object? GetXPathValue(string? culture = null, string? segment = null)
    {
        GetCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel);

        lock (_locko)
        {
            CacheValues cacheValues = GetCacheValues(cacheLevel);
            if (cacheValues.XPathInitialized)
            {
                return cacheValues.XPathValue;
            }

            cacheValues.XPathValue =
                PropertyType.ConvertInterToXPath(Element, referenceCacheLevel, GetInterValue(), IsPreviewing);
            cacheValues.XPathInitialized = true;
            return cacheValues.XPathValue;
        }
    }

    protected class CacheValues
    {
        public bool ObjectInitialized;
        public object? ObjectValue;
        public bool XPathInitialized;
        public object? XPathValue;
    }
}
