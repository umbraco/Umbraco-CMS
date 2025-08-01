using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PublishedCache;

internal sealed class PublishedElementPropertyBase : PublishedPropertyBase
{
    protected readonly IPublishedElement Element;

    // define constant - determines whether to use cache when previewing
    // to store eg routes, property converted values, anything - caching
    // means faster execution, but uses memory - not sure if we want it
    // so making it configurable.
    private readonly Lock _locko = new();
    private readonly object? _sourceValue;
    protected readonly bool IsMember;
    protected readonly bool IsPreviewing;
    private readonly VariationContext _variationContext;
    private readonly ICacheManager? _cacheManager;
    private CacheValues? _cacheValues;

    private bool _interInitialized;
    private object? _interValue;
    private string? _valuesCacheKey;

    public PublishedElementPropertyBase(
        IPublishedPropertyType propertyType,
        IPublishedElement element,
        bool previewing,
        PropertyCacheLevel referenceCacheLevel,
        VariationContext variationContext,
        ICacheManager? cacheManager,
        object? sourceValue = null)
        : base(propertyType, referenceCacheLevel)
    {
        _sourceValue = sourceValue;
        Element = element;
        IsPreviewing = previewing;
        _variationContext = variationContext;
        _cacheManager = cacheManager;
        IsMember = propertyType.ContentType?.ItemType == PublishedItemType.Member;
    }

    // used to cache the CacheValues of this property
    // ReSharper disable InconsistentlySynchronizedField
    private string ValuesCacheKey => _valuesCacheKey ??= PropertyCacheValuesKey();

    [Obsolete("Do not use this. Will be removed in V17.")]
    public static string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing) =>
        "PublishedSnapshot.Property.CacheValues[" + (previewing ? "D:" : "P:") + contentUid + ":" + typeAlias + "]";

    private string PropertyCacheValuesKey() =>
        $"PublishedSnapshot.Property.CacheValues[{(IsPreviewing ? "D:" : "P:")}{Element.Key}:{Alias}:{_variationContext.Culture.IfNullOrWhiteSpace("inv")}+{_variationContext.Segment.IfNullOrWhiteSpace("inv")}]";

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
        => GetCacheLevels(PropertyType.CacheLevel, out cacheLevel, out referenceCacheLevel);

    private void GetDeliveryApiCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel)
        => GetCacheLevels(PropertyType.DeliveryApiCacheLevel, out cacheLevel, out referenceCacheLevel);

    private void GetDeliveryApiCacheLevelsForExpansion(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel)
        => GetCacheLevels(PropertyType.DeliveryApiCacheLevelForExpansion, out cacheLevel, out referenceCacheLevel);

    private void GetCacheLevels(PropertyCacheLevel propertyTypeCacheLevel, out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel)
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
        if (propertyTypeCacheLevel > ReferenceCacheLevel || propertyTypeCacheLevel == PropertyCacheLevel.None)
        {
            cacheLevel = propertyTypeCacheLevel;
            referenceCacheLevel = cacheLevel;
        }
        else
        {
            cacheLevel = PropertyCacheLevel.Element;
            referenceCacheLevel = ReferenceCacheLevel;
        }
    }

    private CacheValues GetCacheValues(PropertyCacheLevel cacheLevel)
    {
        CacheValues cacheValues;
        switch (cacheLevel)
        {
            case PropertyCacheLevel.None:
            case PropertyCacheLevel.Snapshot:
                // never cache anything
                cacheValues = new CacheValues();
                break;
            case PropertyCacheLevel.Element:
                // cache within the property object itself, ie within the content object
                cacheValues = _cacheValues ??= new CacheValues();
                break;
            case PropertyCacheLevel.Elements:
                cacheValues = (CacheValues?)_cacheManager?.ElementsCache.Get(ValuesCacheKey, () => new CacheValues()) ??
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

    public override object? GetDeliveryApiValue(bool expanding, string? culture = null, string? segment = null)
    {
        PropertyCacheLevel cacheLevel, referenceCacheLevel;
        if (expanding)
        {
            GetDeliveryApiCacheLevelsForExpansion(out cacheLevel, out referenceCacheLevel);
        }
        else
        {
            GetDeliveryApiCacheLevels(out cacheLevel, out referenceCacheLevel);
        }

        lock (_locko)
        {
            CacheValues cacheValues = GetCacheValues(cacheLevel);

            object? GetDeliveryApiObject() => PropertyType.ConvertInterToDeliveryApiObject(Element, referenceCacheLevel, GetInterValue(), IsPreviewing, expanding);
            return expanding
                ? GetDeliveryApiExpandedObject(cacheValues, GetDeliveryApiObject)
                : GetDeliveryApiDefaultObject(cacheValues, GetDeliveryApiObject);
        }
    }

    private object? GetDeliveryApiDefaultObject(CacheValues cacheValues, Func<object?> getValue)
    {
        if (cacheValues.DeliveryApiDefaultObjectInitialized == false)
        {
            cacheValues.DeliveryApiDefaultObjectValue = getValue();
            cacheValues.DeliveryApiDefaultObjectInitialized = true;
        }

        return cacheValues.DeliveryApiDefaultObjectValue;
    }

    private object? GetDeliveryApiExpandedObject(CacheValues cacheValues, Func<object?> getValue)
    {
        if (cacheValues.DeliveryApiExpandedObjectInitialized == false)
        {
            cacheValues.DeliveryApiExpandedObjectValue = getValue();
            cacheValues.DeliveryApiExpandedObjectInitialized = true;
        }

        return cacheValues.DeliveryApiExpandedObjectValue;
    }

    protected class CacheValues
    {
        public bool ObjectInitialized;
        public object? ObjectValue;
        public bool XPathInitialized;
        public object? XPathValue;
        public bool DeliveryApiDefaultObjectInitialized;
        public object? DeliveryApiDefaultObjectValue;
        public bool DeliveryApiExpandedObjectInitialized;
        public object? DeliveryApiExpandedObjectValue;
    }
}
