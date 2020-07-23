﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Collections;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    internal class Property : PublishedPropertyBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly Guid _contentUid;
        private readonly bool _isPreviewing;
        private readonly bool _isMember;
        private readonly PublishedContent _content;
        private readonly ContentVariation _variations;

        private readonly object _locko = new object();

        // the invariant-neutral source and inter values
        private readonly object _sourceValue;
        private bool _interInitialized;
        private object _interValue;

        // the variant source and inter values
        private Dictionary<CompositeStringStringKey, SourceInterValue> _sourceValues;

        // the variant and non-variant object values
        private CacheValues _cacheValues;

        private string _valuesCacheKey;

        // initializes a published content property with no value
        public Property(IPublishedPropertyType propertyType, PublishedContent content, IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
            : this(propertyType, content, null, publishedSnapshotAccessor, referenceCacheLevel)
        { }

        // initializes a published content property with a value
        public Property(IPublishedPropertyType propertyType, PublishedContent content, PropertyData[] sourceValues, IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
            : base(propertyType, referenceCacheLevel)
        {
            if (sourceValues != null)
            {
                foreach (var sourceValue in sourceValues)
                {
                    if (sourceValue.Culture == "" && sourceValue.Segment == "")
                    {
                        _sourceValue = sourceValue.Value;
                    }
                    else
                    {
                        if (_sourceValues == null)
                            _sourceValues = new Dictionary<CompositeStringStringKey, SourceInterValue>();
                        _sourceValues[new CompositeStringStringKey(sourceValue.Culture, sourceValue.Segment)]
                            = new SourceInterValue { Culture = sourceValue.Culture, Segment = sourceValue.Segment, SourceValue = sourceValue.Value };
                    }
                }
            }

            _contentUid = content.Key;
            _content = content;
            _isPreviewing = content.IsPreviewing;
            _isMember = content.ContentType.ItemType == PublishedItemType.Member;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _variations = propertyType.Variations;
        }

        // clone for previewing as draft a published content that is published and has no draft
        public Property(Property origin, PublishedContent content)
            : base(origin.PropertyType, origin.ReferenceCacheLevel)
        {
            _sourceValue = origin._sourceValue;
            _sourceValues = origin._sourceValues;

            _contentUid = origin._contentUid;
            _content = content;
            _isPreviewing = true;
            _isMember = origin._isMember;
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
            _variations = origin._variations;
        }

        // determines whether a property has value
        public override bool HasValue(string culture = null, string segment = null)
        {
            _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);

            var value = GetSourceValue(culture, segment);
            var hasValue = PropertyType.IsValue(value, PropertyValueLevel.Source);
            if (hasValue.HasValue) return hasValue.Value;

            lock (_locko)
            {
                value = GetInterValue(culture, segment);
                hasValue = PropertyType.IsValue(value, PropertyValueLevel.Inter);
                if (hasValue.HasValue) return hasValue.Value;

                var cacheValues = GetCacheValues(PropertyType.CacheLevel).For(culture, segment);

                // initial reference cache level always is .Content
                const PropertyCacheLevel initialCacheLevel = PropertyCacheLevel.Element;

                if (!cacheValues.ObjectInitialized)
                {
                    cacheValues.ObjectValue = PropertyType.ConvertInterToObject(_content, initialCacheLevel, value, _isPreviewing);
                    cacheValues.ObjectInitialized = true;
                }
                value = cacheValues.ObjectValue;
                return PropertyType.IsValue(value, PropertyValueLevel.Object) ?? false;
            }
        }

        // used to cache the CacheValues of this property
        internal string ValuesCacheKey => _valuesCacheKey
            ?? (_valuesCacheKey = CacheKeys.PropertyCacheValues(_contentUid, Alias, _isPreviewing));

        private CacheValues GetCacheValues(PropertyCacheLevel cacheLevel)
        {
            CacheValues cacheValues;
            PublishedSnapshot publishedSnapshot;
            IAppCache cache;
            switch (cacheLevel)
            {
                case PropertyCacheLevel.None:
                    // never cache anything
                    cacheValues = new CacheValues();
                    break;
                case PropertyCacheLevel.Element:
                    // cache within the property object itself, ie within the content object
                    cacheValues = _cacheValues ?? (_cacheValues = new CacheValues());
                    break;
                case PropertyCacheLevel.Elements:
                    // cache within the elements cache, unless previewing, then use the snapshot or
                    // elements cache (if we don't want to pollute the elements cache with short-lived
                    // data) depending on settings
                    // for members, always cache in the snapshot cache - never pollute elements cache
                    publishedSnapshot = (PublishedSnapshot) _publishedSnapshotAccessor.PublishedSnapshot;
                    cache = publishedSnapshot == null
                        ? null
                        : ((_isPreviewing == false || PublishedSnapshotService.FullCacheWhenPreviewing) && (_isMember == false)
                            ? publishedSnapshot.ElementsCache
                            : publishedSnapshot.SnapshotCache);
                    cacheValues = GetCacheValues(cache);
                    break;
                case PropertyCacheLevel.Snapshot:
                    // cache within the snapshot cache
                    publishedSnapshot = (PublishedSnapshot) _publishedSnapshotAccessor.PublishedSnapshot;
                    cache = publishedSnapshot?.SnapshotCache;
                    cacheValues = GetCacheValues(cache);
                    break;
                default:
                    throw new InvalidOperationException("Invalid cache level.");
            }
            return cacheValues;
        }

        private CacheValues GetCacheValues(IAppCache cache)
        {
            if (cache == null) // no cache, don't cache
                return new CacheValues();
            return (CacheValues) cache.Get(ValuesCacheKey, () => new CacheValues());
        }

        // this is always invoked from within a lock, so does not require its own lock
        private object GetInterValue(string culture, string segment)
        {
            if (culture == "" && segment == "")
            {
                if (_interInitialized) return _interValue;
                _interValue = PropertyType.ConvertSourceToInter(_content, _sourceValue, _isPreviewing);
                _interInitialized = true;
                return _interValue;
            }

            if (_sourceValues == null)
                _sourceValues = new Dictionary<CompositeStringStringKey, SourceInterValue>();

            var k = new CompositeStringStringKey(culture, segment);
            if (!_sourceValues.TryGetValue(k, out var vvalue))
                _sourceValues[k] = vvalue = new SourceInterValue { Culture = culture, Segment = segment, SourceValue = GetSourceValue(culture, segment) };

            if (vvalue.InterInitialized) return vvalue.InterValue;
            vvalue.InterValue = PropertyType.ConvertSourceToInter(_content, vvalue.SourceValue, _isPreviewing);
            vvalue.InterInitialized = true;
            return vvalue.InterValue;
        }

        public override object GetSourceValue(string culture = null, string segment = null)
        {
            _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);

            if (culture == "" && segment == "")
                return _sourceValue;

            lock (_locko)
            {
                if (_sourceValues == null) return null;
                return _sourceValues.TryGetValue(new CompositeStringStringKey(culture, segment), out var sourceValue) ? sourceValue.SourceValue : null;
            }
        }

        public override object GetValue(string culture = null, string segment = null)
        {
            _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);

            object value;
            lock (_locko)
            {
                var cacheValues = GetCacheValues(PropertyType.CacheLevel).For(culture, segment);

                // initial reference cache level always is .Content
                const PropertyCacheLevel initialCacheLevel = PropertyCacheLevel.Element;

                if (cacheValues.ObjectInitialized) return cacheValues.ObjectValue;
                cacheValues.ObjectValue = PropertyType.ConvertInterToObject(_content, initialCacheLevel, GetInterValue(culture, segment), _isPreviewing);
                cacheValues.ObjectInitialized = true;
                value = cacheValues.ObjectValue;
            }

            return value;
        }

        public override object GetXPathValue(string culture = null, string segment = null)
        {
            _content.VariationContextAccessor.ContextualizeVariation(_variations, _content.Id, ref culture, ref segment);

            lock (_locko)
            {
                var cacheValues = GetCacheValues(PropertyType.CacheLevel).For(culture, segment);

                // initial reference cache level always is .Content
                const PropertyCacheLevel initialCacheLevel = PropertyCacheLevel.Element;

                if (cacheValues.XPathInitialized) return cacheValues.XPathValue;
                cacheValues.XPathValue = PropertyType.ConvertInterToXPath(_content, initialCacheLevel, GetInterValue(culture, segment), _isPreviewing);
                cacheValues.XPathInitialized = true;
                return cacheValues.XPathValue;
            }
        }

        #region Classes

        private class CacheValue
        {
            public bool ObjectInitialized { get; set; }
            public object ObjectValue { get; set; }
            public bool XPathInitialized { get; set; }
            public object XPathValue { get; set; }
        }

        private class CacheValues : CacheValue
        {
            private Dictionary<CompositeStringStringKey, CacheValue> _values;

            // this is always invoked from within a lock, so does not require its own lock
            public CacheValue For(string culture, string segment)
            {
                if (culture == "" && segment == "")
                    return this;

                if (_values == null)
                    _values = new Dictionary<CompositeStringStringKey, CacheValue>();

                var k = new CompositeStringStringKey(culture, segment);
                if (!_values.TryGetValue(k, out var value))
                    _values[k] = value = new CacheValue();

                return value;
            }
        }

        private class SourceInterValue
        {
            private string _culture;
            private string _segment;

            public string Culture
            {
                get => _culture;
                internal set => _culture = value?.ToLowerInvariant();
            }
            public string Segment
            {
                get => _segment;
                internal set => _segment = value?.ToLowerInvariant();
            }
            public object SourceValue { get; set; }
            public bool InterInitialized { get; set; }
            public object InterValue { get; set; }
        }

        #endregion
    }
}
