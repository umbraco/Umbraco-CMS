using System.Collections.Concurrent;
using System.Xml.Serialization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

[Serializable]
[XmlType(Namespace = "http://umbraco.org/webservices/")]
internal class PublishedProperty : PublishedPropertyBase
{
    private readonly PublishedContent _content;
    private readonly bool _isPreviewing;

    // the invariant-neutral source and inter values
    private readonly object? _sourceValue;
    private readonly ContentVariation _variations;
    private readonly ContentVariation _sourceVariations;

    // the variant and non-variant object values
    private bool _interInitialized;
    private object? _interValue;

    // the variant source and inter values
    private readonly object _locko = new();
    private ConcurrentDictionary<CompositeStringStringKey, SourceInterValue>? _sourceValues;

    // initializes a published content property with no value
    public PublishedProperty(
        IPublishedPropertyType propertyType,
        PublishedContent content,
        PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.None)
        : this(propertyType, content, null, referenceCacheLevel)
    {
    }

    // initializes a published content property with a value
    public PublishedProperty(
        IPublishedPropertyType propertyType,
        PublishedContent content,
        PropertyData[]? sourceValues,
        PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.None)
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

        // this variable is used for contextualizing the variation level when calculating property values.
        // it must be set to the union of variance (the combination of content type and property type variance).
        _variations = propertyType.Variations | content.ContentType.Variations;
        _sourceVariations = propertyType.Variations;
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

        return PropertyType.ConvertInterToObject(_content, PropertyCacheLevel.None, GetInterValue(culture, segment), _isPreviewing);
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
