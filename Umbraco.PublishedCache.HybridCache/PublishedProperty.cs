using System.Collections.Concurrent;
using System.Xml.Serialization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Snapshot;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

[Serializable]
[XmlType(Namespace = "http://umbraco.org/webservices/")]
internal class PublishedProperty : PublishedPropertyBase
{
    private readonly PublishedContent _content;
    private readonly bool _isPreviewing;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly bool _isMember;
    private string? _valuesCacheKey;

    // the invariant-neutral source and inter values
    private readonly object? _sourceValue;
    private readonly ContentVariation _variations;
    private readonly ContentVariation _sourceVariations;

    // the variant and non-variant object values
    private bool _interInitialized;
    private object? _interValue;
    private CacheValues? _cacheValues;

    // the variant source and inter values
    private readonly object _locko = new();
    private ConcurrentDictionary<CompositeStringStringKey, SourceInterValue>? _sourceValues;

    // initializes a published content property with no value
    public PublishedProperty(
        IPublishedPropertyType propertyType,
        PublishedContent content,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
        : this(propertyType, content, null, publishedSnapshotAccessor, referenceCacheLevel)
    {
    }

    // initializes a published content property with a value
    public PublishedProperty(
        IPublishedPropertyType propertyType,
        PublishedContent content,
        PropertyData[]? sourceValues,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
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

        _content = content;
        _isPreviewing = content.IsPreviewing;
        _isMember = content.ContentType.ItemType == PublishedItemType.Member;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;

        // this variable is used for contextualizing the variation level when calculating property values.
        // it must be set to the union of variance (the combination of content type and property type variance).
        _variations = propertyType.Variations | content.ContentType.Variations;
        _sourceVariations = propertyType.Variations;
    }

    // used to cache the CacheValues of this property
    internal string ValuesCacheKey => _valuesCacheKey ??= PropertyCacheValues(_content.Key, Alias, _isPreviewing);

    private string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing)
    {
        if (previewing)
        {
            return "Cache.Property.CacheValues[D:" + contentUid + ":" + typeAlias + "]";
        }

        return "Cache.Property.CacheValues[P:" + contentUid + ":" + typeAlias + "]";
    }

    // determines whether a property has value
    public override bool HasValue(string? culture = null, string? segment = null)
    {
        _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);

        var value = GetSourceValue(culture, segment);
        var hasValue = PropertyType.IsValue(value, PropertyValueLevel.Source);
        if (hasValue.HasValue)
        {
            return hasValue.Value;
        }

        return PropertyType.IsValue(GetInterValue(culture, segment), PropertyValueLevel.Object) ?? false;
    }

    public override object? GetSourceValue(string? culture = null, string? segment = null)
    {
        _content.VariationContextAccessor.ContextualizeVariation(_sourceVariations, _content.Id, ref culture, ref segment);

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

            _interValue = PropertyType.ConvertSourceToInter(_content, _sourceValue, _isPreviewing);
            _interInitialized = true;
            return _interValue;
        }

        return PropertyType.ConvertSourceToInter(_content, GetSourceValue(culture, segment), _isPreviewing);
    }

    public override object? GetValue(string? culture = null, string? segment = null)
    {
        _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);

        object? value;
        CacheValue cacheValues = GetCacheValues(PropertyType.CacheLevel).For(culture, segment);

        // initial reference cache level always is .Content
        const PropertyCacheLevel initialCacheLevel = PropertyCacheLevel.Element;

        if (cacheValues.ObjectInitialized)
        {
            return cacheValues.ObjectValue;
        }

        cacheValues.ObjectValue = PropertyType.ConvertInterToObject(_content, initialCacheLevel, GetInterValue(culture, segment), _isPreviewing);
        cacheValues.ObjectInitialized = true;
        value = cacheValues.ObjectValue;

        return value;
    }

    private CacheValues GetCacheValues(PropertyCacheLevel cacheLevel)
    {
        CacheValues cacheValues;
        IPublishedSnapshot publishedSnapshot;
        IAppCache? cache;
        switch (cacheLevel)
        {
            case PropertyCacheLevel.None:
                // never cache anything
                cacheValues = new CacheValues();
                break;
            case PropertyCacheLevel.Snapshot: // Snapshot is obsolete, so for now treat as element
            case PropertyCacheLevel.Element:
                // cache within the property object itself, ie within the content object
                cacheValues = _cacheValues ??= new CacheValues();
                break;
            case PropertyCacheLevel.Elements:
                // cache within the elements cache, unless previewing, then use the snapshot or
                // elements cache (if we don't want to pollute the elements cache with short-lived
                // data) depending on settings
                // for members, always cache in the snapshot cache - never pollute elements cache
                publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
                cache = publishedSnapshot == null ? null : _isMember == false
                        ? publishedSnapshot.ElementsCache
                        : null;
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
        _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);
        return PropertyType.ConvertInterToDeliveryApiObject(_content, PropertyCacheLevel.None, GetInterValue(culture, segment), _isPreviewing, expanding);
    }

    private class SourceInterValue
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

    private class CacheValues : CacheValue
    {
        private readonly object _locko = new();
        private ConcurrentDictionary<CompositeStringStringKey, CacheValue>? _values;

        public CacheValue For(string? culture, string? segment)
        {
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

        public bool XPathInitialized { get; set; }

        public object? XPathValue { get; set; }

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
