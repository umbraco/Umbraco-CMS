using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Represents a property of a published element with caching support for property value conversions.
/// </summary>
internal sealed class PublishedElementPropertyBase : PublishedPropertyBase
{
    private readonly IPublishedElement _element;

    // define constant - determines whether to use cache when previewing
    // to store eg routes, property converted values, anything - caching
    // means faster execution, but uses memory - not sure if we want it
    // so making it configurable.
    private readonly Lock _cacheLock = new();
    private readonly object? _sourceValue;
    private readonly bool _isMember;
    private readonly bool _isPreviewing;
    private readonly VariationContext _variationContext;
    private readonly ICacheManager? _cacheManager;
    private CacheValues? _cacheValues;

    private bool _interInitialized;
    private object? _interValue;
    private string? _valuesCacheKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElementPropertyBase"/> class.
    /// </summary>
    /// <param name="propertyType">The published property type.</param>
    /// <param name="element">The published element that owns this property.</param>
    /// <param name="previewing">Whether this is a preview request.</param>
    /// <param name="referenceCacheLevel">The reference cache level.</param>
    /// <param name="variationContext">The variation context for culture and segment.</param>
    /// <param name="cacheManager">The cache manager.</param>
    /// <param name="sourceValue">The source value of the property.</param>
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
        _element = element;
        _isPreviewing = previewing;
        _variationContext = variationContext;
        _cacheManager = cacheManager;
        _isMember = propertyType.ContentType?.ItemType == PublishedItemType.Member;
    }

    // used to cache the CacheValues of this property
    // ReSharper disable InconsistentlySynchronizedField
    private string ValuesCacheKey => _valuesCacheKey ??= PropertyCacheValuesKey();

    private string PropertyCacheValuesKey() =>
        $"PublishedSnapshot.Property.CacheValues[{(_isPreviewing ? "D:" : "P:")}{_element.Key}:{Alias}:{_variationContext.Culture.IfNullOrWhiteSpace("inv")}+{_variationContext.Segment.IfNullOrWhiteSpace("inv")}]";

    // ReSharper restore InconsistentlySynchronizedField

    /// <inheritdoc />
    public override bool HasValue(string? culture = null, string? segment = null)
    {
        var hasValue = PropertyType.IsValue(_sourceValue, PropertyValueLevel.Source);
        if (hasValue.HasValue)
        {
            return hasValue.Value;
        }

        GetCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel);

        lock (_cacheLock)
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
                    PropertyType.ConvertInterToObject(_element, referenceCacheLevel, value, _isPreviewing);
                cacheValues.ObjectInitialized = true;
            }

            value = cacheValues.ObjectValue;
            return PropertyType.IsValue(value, PropertyValueLevel.Object) ?? false;
        }
    }

    /// <inheritdoc />
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

        _interValue = PropertyType.ConvertSourceToInter(_element, _sourceValue, _isPreviewing);
        _interInitialized = true;
        return _interValue;
    }

    /// <inheritdoc />
    public override object? GetValue(string? culture = null, string? segment = null)
    {
        GetCacheLevels(out PropertyCacheLevel cacheLevel, out PropertyCacheLevel referenceCacheLevel);

        lock (_cacheLock)
        {
            CacheValues cacheValues = GetCacheValues(cacheLevel);
            if (cacheValues.ObjectInitialized)
            {
                return cacheValues.ObjectValue;
            }

            cacheValues.ObjectValue =
                PropertyType.ConvertInterToObject(_element, referenceCacheLevel, GetInterValue(), _isPreviewing);
            cacheValues.ObjectInitialized = true;
            return cacheValues.ObjectValue;
        }
    }

    /// <inheritdoc />
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

        lock (_cacheLock)
        {
            CacheValues cacheValues = GetCacheValues(cacheLevel);

            object? GetDeliveryApiObject() => PropertyType.ConvertInterToDeliveryApiObject(_element, referenceCacheLevel, GetInterValue(), _isPreviewing, expanding);
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

    private class CacheValues
    {
        public bool ObjectInitialized { get; set; }
        public object? ObjectValue { get; set; }
        public bool XPathInitialized { get; set; }
        public object? XPathValue { get; set; }
        public bool DeliveryApiDefaultObjectInitialized { get; set; }
        public object? DeliveryApiDefaultObjectValue { get; set; }
        public bool DeliveryApiExpandedObjectInitialized { get; set; }
        public object? DeliveryApiExpandedObjectValue { get; set; }
    }
}
