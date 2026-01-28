using System.Collections.Concurrent;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class PublishedProperty : PublishedPropertyBase
{
    private readonly IPublishedElement _element;
    private readonly bool _isPreviewing;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IElementsCache _elementsCache;
    private readonly bool _isMember;
    private string? _valuesCacheKey;

    // the invariant-neutral source and inter values
    private readonly object? _sourceValue;
    private readonly ContentVariation _variations;
    private readonly ContentVariation _sourceVariations;

    private readonly string _propertyTypeAlias;

    // the variant and non-variant object values
    private bool _interInitialized;
    private object? _interValue;
    private CacheValues? _cacheValues;

    // the variant source and inter values
    private readonly Lock _locko = new();
    private ConcurrentDictionary<CompositeStringStringKey, SourceInterValue>? _sourceValues;

    // initializes a published content property with a value
    public PublishedProperty(
        IPublishedPropertyType propertyType,
        IPublishedElement element,
        IVariationContextAccessor variationContextAccessor,
        bool preview,
        PropertyData[]? sourceValues,
        IElementsCache elementsElementsCache,
        PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
        : base(propertyType, referenceCacheLevel)
    {
        if (sourceValues != null)
        {
            foreach (PropertyData sourceValue in sourceValues)
            {
                if (sourceValue.Culture == string.Empty && sourceValue.Segment == string.Empty)
                {
                    _sourceValue = sourceValue.Value;
                }
                else
                {
                    EnsureSourceValuesInitialized();

                    _sourceValues![new CompositeStringStringKey(sourceValue.Culture, sourceValue.Segment)]
                        = new SourceInterValue
                        {
                            Culture = sourceValue.Culture,
                            Segment = sourceValue.Segment,
                            SourceValue = sourceValue.Value,
                        };
                }
            }
        }

        _element = element;
        _variationContextAccessor = variationContextAccessor;
        _isPreviewing = preview;
        _isMember = element.ContentType.ItemType == PublishedItemType.Member;
        _elementsCache = elementsElementsCache;

        // this variable is used for contextualizing the variation level when calculating property values.
        // it must be set to the union of variance (the combination of content type and property type variance).
        _variations = propertyType.Variations | element.ContentType.Variations;
        _sourceVariations = propertyType.Variations;

        _propertyTypeAlias = propertyType.Alias;
    }

    // used to cache the CacheValues of this property
    internal string ValuesCacheKey => _valuesCacheKey ??= PropertyCacheValues(_element.Key, Alias, _isPreviewing);

    private static string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing)
    {
        if (previewing)
        {
            return CacheKeys.PreviewPropertyCacheKeyPrefix + contentUid + ":" + typeAlias + "]";
        }

        return CacheKeys.PropertyCacheKeyPrefix + contentUid + ":" + typeAlias + "]";
    }

    // determines whether a property has value
    public override bool HasValue(string? culture = null, string? segment = null)
    {
        _variationContextAccessor.ContextualizeVariation(_variations, _element.Id, ref culture, ref segment);

        var value = GetSourceValue(culture, segment);
        var isValue = PropertyType.IsValue(value, PropertyValueLevel.Source);
        if (isValue.HasValue)
        {
            return isValue.Value;
        }

        value = GetInterValue(culture, segment);
        isValue = PropertyType.IsValue(value, PropertyValueLevel.Inter);
        if (isValue.HasValue)
        {
            return isValue.Value;
        }

        value = GetValue(culture, segment);
        isValue = PropertyType.IsValue(value, PropertyValueLevel.Object);

        return isValue ?? false;
    }

    public override object? GetSourceValue(string? culture = null, string? segment = null)
    {
        _variationContextAccessor.ContextualizeVariation(_sourceVariations, _element.Id, ref culture, ref segment);

        // source values are tightly bound to the property/schema culture and segment configurations, so we need to
        // sanitize the contextualized culture/segment states before using them to access the source values.
        culture = _sourceVariations.VariesByCulture() ? culture : string.Empty;
        segment = _sourceVariations.VariesBySegment() ? segment : string.Empty;

        if (culture == string.Empty && segment == string.Empty)
        {
            return _sourceValue;
        }

        if (_sourceValues == null)
        {
            return null;
        }

        return _sourceValues.TryGetValue(
            new CompositeStringStringKey(culture, segment),
            out SourceInterValue? sourceValue)
            ? sourceValue.SourceValue
            : null;
    }

    private object? GetInterValue(string? culture, string? segment)
    {
        if (culture is "" && segment is "")
        {
            if (_interInitialized)
            {
                return _interValue;
            }

            _interValue = PropertyType.ConvertSourceToInter(_element, _sourceValue, _isPreviewing);
            _interInitialized = true;
            return _interValue;
        }

        return PropertyType.ConvertSourceToInter(_element, GetSourceValue(culture, segment), _isPreviewing);
    }

    public override object? GetValue(string? culture = null, string? segment = null)
    {
        _variationContextAccessor.ContextualizeVariation(_variations, _element.Id, ref culture, ref segment);

        object? value;
        CacheValue cacheValues = GetCacheValues(PropertyType.CacheLevel).For(culture, segment);

        // initial reference cache level always is .Content
        const PropertyCacheLevel initialCacheLevel = PropertyCacheLevel.Element;

        if (cacheValues.ObjectInitialized)
        {
            return cacheValues.ObjectValue;
        }

        cacheValues.ObjectValue = PropertyType.ConvertInterToObject(_element, initialCacheLevel, GetInterValue(culture, segment), _isPreviewing);
        cacheValues.ObjectInitialized = true;
        value = cacheValues.ObjectValue;

        return value;
    }

    private CacheValues GetCacheValues(PropertyCacheLevel cacheLevel)
    {
        CacheValues cacheValues;
        IAppCache? cache;
        switch (cacheLevel)
        {
            case PropertyCacheLevel.None:
            case PropertyCacheLevel.Snapshot: // Snapshot is obsolete, so for now treat as None
                // never cache anything
                cacheValues = new CacheValues();
                break;
            case PropertyCacheLevel.Element:
                // cache within the property object itself, ie within the content object
                cacheValues = _cacheValues ??= new CacheValues();
                break;
            case PropertyCacheLevel.Elements:
                // cache within the elements cache, unless previewing, then use the snapshot or
                // elements cache (if we don't want to pollute the elements cache with short-lived
                // data) depending on settings
                // for members, always cache in the snapshot cache - never pollute elements cache
                cache = _isMember == false ? _elementsCache : null;
                cacheValues = GetCacheValues(cache);
                break;
            default:
                throw new InvalidOperationException("Invalid cache level.");
        }

        return cacheValues;
    }

    private CacheValues GetCacheValues(IAppCache? cache)
    {
        // no cache, don't cache
        if (cache == null)
        {
            return new CacheValues();
        }

        return (CacheValues)cache.Get(ValuesCacheKey, () => new CacheValues())!;
    }

    public override object? GetDeliveryApiValue(bool expanding, string? culture = null, string? segment = null)
    {
        _variationContextAccessor.ContextualizeVariation(_variations, _element.Id, ref culture, ref segment);

        object? value;
        CacheValue cacheValues = GetCacheValues(expanding ? PropertyType.DeliveryApiCacheLevelForExpansion : PropertyType.DeliveryApiCacheLevel).For(culture, segment);


        // initial reference cache level always is .Content
        const PropertyCacheLevel initialCacheLevel = PropertyCacheLevel.Element;

        object? GetDeliveryApiObject() => PropertyType.ConvertInterToDeliveryApiObject(_element, initialCacheLevel, GetInterValue(culture, segment), _isPreviewing, expanding);
        value = expanding
            ? GetDeliveryApiExpandedObject(cacheValues, GetDeliveryApiObject)
            : GetDeliveryApiDefaultObject(cacheValues, GetDeliveryApiObject);

        return value;
    }

    private static object? GetDeliveryApiDefaultObject(CacheValue cacheValues, Func<object?> getValue)
    {
        if (cacheValues.DeliveryApiDefaultObjectInitialized == false)
        {
            cacheValues.DeliveryApiDefaultObjectValue = getValue();
            cacheValues.DeliveryApiDefaultObjectInitialized = true;
        }

        return cacheValues.DeliveryApiDefaultObjectValue;
    }

    private static object? GetDeliveryApiExpandedObject(CacheValue cacheValues, Func<object?> getValue)
    {
        if (cacheValues.DeliveryApiExpandedObjectInitialized == false)
        {
            cacheValues.DeliveryApiExpandedObjectValue = getValue();
            cacheValues.DeliveryApiExpandedObjectInitialized = true;
        }

        return cacheValues.DeliveryApiExpandedObjectValue;
    }

    private sealed class SourceInterValue
    {
        private string? _culture;
        private string? _segment;

        public string? Culture
        {
            get => _culture;
            internal set => _culture = value?.ToLowerInvariant();
        }

        public string? Segment
        {
            get => _segment;
            internal set => _segment = value?.ToLowerInvariant();
        }

        public object? SourceValue { get; set; }
    }

    private sealed class CacheValues : CacheValue
    {
        private readonly Lock _locko = new();
        private ConcurrentDictionary<CompositeStringStringKey, CacheValue>? _values;

        public CacheValue For(string? culture, string? segment)
        {
            // As noted on IPropertyValue, null value means invariant
            // But as we need an actual string value to build a CompositeStringStringKey
            // We need to convert null to empty
            culture ??= string.Empty;
            segment ??= string.Empty;

            if (culture == string.Empty && segment == string.Empty)
            {
                return this;
            }

            if (_values == null)
            {
                lock (_locko)
                {
                    _values ??= InitializeConcurrentDictionary<CompositeStringStringKey, CacheValue>();
                }
            }

            var k = new CompositeStringStringKey(culture, segment);

            CacheValue value = _values.GetOrAdd(k, _ => new CacheValue());

            return value;
        }
    }

    private class CacheValue
    {
        public bool ObjectInitialized { get; set; }

        public object? ObjectValue { get; set; }

        public bool DeliveryApiDefaultObjectInitialized { get; set; }

        public object? DeliveryApiDefaultObjectValue { get; set; }

        public bool DeliveryApiExpandedObjectInitialized { get; set; }

        public object? DeliveryApiExpandedObjectValue { get; set; }
    }

    private static ConcurrentDictionary<TKey, TValue> InitializeConcurrentDictionary<TKey, TValue>()
        where TKey : notnull
        => new(-1, 5);

    private void EnsureSourceValuesInitialized()
    {
        if (_sourceValues is not null)
        {
            return;
        }

        lock (_locko)
        {
            _sourceValues ??= InitializeConcurrentDictionary<CompositeStringStringKey, SourceInterValue>();
        }
    }
}
