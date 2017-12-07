using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Umbraco.Core.Cache;
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
        private readonly IPublishedContent _content;

        private readonly object _locko = new object();

        private readonly object _sourceValue;
        private bool _interInitialized;
        private object _interValue;

        private List<PropertyValue> _sourceVValues;

        private CacheValues _cacheValues;

        private string _valuesCacheKey;
        private string _recurseCacheKey;

        // initializes a published content property with no value
        public Property(PublishedPropertyType propertyType, PublishedContent content, IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
            : this(propertyType, content, null, publishedSnapshotAccessor, referenceCacheLevel)
        { }

        // initializes a published content property with a value
        public Property(PublishedPropertyType propertyType, PublishedContent content, PropertyData[] sourceValues, IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
            : base(propertyType, referenceCacheLevel)
        {
            if (sourceValues != null)
            {
                foreach (var sourceValue in sourceValues)
                {
                    if (sourceValue.LanguageId == null && sourceValue.Segment == null)
                    {
                        _sourceValue = sourceValue.Value;
                    }
                    else
                    {
                        if (_sourceVValues == null)
                            _sourceVValues = new List<PropertyValue>();
                        _sourceVValues.Add(new PropertyValue { LanguageId = sourceValue.LanguageId, Segment = sourceValue.Segment, SourceValue = sourceValue.Value });
                    }
                }
            }

            _contentUid = content.Key;
            _content = content;
            _isPreviewing = content.IsPreviewing;
            _isMember = content.ContentType.ItemType == PublishedItemType.Member;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
        }

        // clone for previewing as draft a published content that is published and has no draft
        public Property(Property origin, IPublishedContent content)
            : base(origin.PropertyType, origin.ReferenceCacheLevel)
        {
            _sourceValue = origin._sourceValue;
            _sourceVValues = origin._sourceVValues;

            _contentUid = origin._contentUid;
            _content = content;
            _isPreviewing = true;
            _isMember = origin._isMember;
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
        }

        public override bool HasValue(int? languageId = null, string segment = null) => _sourceValue != null
            && (!(_sourceValue is string) || string.IsNullOrWhiteSpace((string) _sourceValue) == false);

        // used to cache the recursive *property* for this property
        internal string RecurseCacheKey => _recurseCacheKey
            ?? (_recurseCacheKey = CacheKeys.PropertyRecurse(_contentUid, PropertyTypeAlias, _isPreviewing));

        // used to cache the CacheValues of this property
        internal string ValuesCacheKey => _valuesCacheKey
            ?? (_valuesCacheKey = CacheKeys.PropertyCacheValues(_contentUid, PropertyTypeAlias, _isPreviewing));

        private CacheValues GetCacheValues(PropertyCacheLevel cacheLevel)
        {
            CacheValues cacheValues;
            PublishedShapshot publishedSnapshot;
            ICacheProvider cache;
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
                    publishedSnapshot = (PublishedShapshot) _publishedSnapshotAccessor.PublishedSnapshot;
                    cache = publishedSnapshot == null
                        ? null
                        : ((_isPreviewing == false || PublishedSnapshotService.FullCacheWhenPreviewing) && (_isMember == false)
                            ? publishedSnapshot.ElementsCache
                            : publishedSnapshot.SnapshotCache);
                    cacheValues = GetCacheValues(cache);
                    break;
                case PropertyCacheLevel.Snapshot:
                    // cache within the snapshot cache
                    publishedSnapshot = (PublishedShapshot) _publishedSnapshotAccessor.PublishedSnapshot;
                    cache = publishedSnapshot?.SnapshotCache;
                    cacheValues = GetCacheValues(cache);
                    break;
                default:
                    throw new InvalidOperationException("Invalid cache level.");
            }
            return cacheValues;
        }

        private CacheValues GetCacheValues(ICacheProvider cache)
        {
            if (cache == null) // no cache, don't cache
                return new CacheValues();
            return (CacheValues) cache.GetCacheItem(ValuesCacheKey, () => new CacheValues());
        }

        // this is always invoked from within a lock, so does not require its own lock
        private object GetInterValue(int? languageId, string segment)
        {
            if (languageId == null && segment == null)
            {
                if (_interInitialized) return _interValue;

                _interValue = PropertyType.ConvertSourceToInter(_content, _sourceValue, _isPreviewing);
                _interInitialized = true;
                return _interValue;
            }

            if (_sourceVValues == null)
                _sourceVValues = new List<PropertyValue>();

            var vvalue = _sourceVValues.FirstOrDefault(x => x.LanguageId == languageId && x.Segment == segment);

            if (vvalue == null)
                _sourceVValues.Add(vvalue = new PropertyValue { LanguageId = languageId, Segment = segment });

            if (vvalue.InterInitialized) return vvalue.InterValue;

            vvalue.InterValue = PropertyType.ConvertSourceToInter(_content, vvalue.SourceValue, _isPreviewing);
            vvalue.InterInitialized = true;
            return vvalue.InterValue;
        }

        public override object GetSourceValue(int? languageId = null, string segment = null)
        {
            if (languageId == null && segment == null)
                return _sourceValue;

            List<PropertyValue> vvalues;
            lock (_locko)
            {
                vvalues = _sourceVValues;
            }

            return vvalues?.FirstOrDefault(x => x.LanguageId == languageId && x.Segment == segment);
        }

        public override object GetValue(int? languageId = null, string segment = null)
        {
            lock (_locko)
            {
                var cacheValues = GetCacheValues(PropertyType.CacheLevel).For(languageId, segment);
                if (cacheValues.ObjectInitialized) return cacheValues.ObjectValue;

                // initial reference cache level always is .Content
                cacheValues.ObjectValue = PropertyType.ConvertInterToObject(_content, PropertyCacheLevel.Element, GetInterValue(languageId, segment), _isPreviewing);
                cacheValues.ObjectInitialized = true;
                return cacheValues.ObjectValue;
            }
        }

        public override object GetXPathValue(int? languageId = null, string segment = null)
        {
            lock (_locko)
            {
                var cacheValues = GetCacheValues(PropertyType.CacheLevel).For(languageId, segment);
                if (cacheValues.XPathInitialized) return cacheValues.XPathValue;

                // initial reference cache level always is .Content
                cacheValues.XPathValue = PropertyType.ConvertInterToXPath(_content, PropertyCacheLevel.Element, GetInterValue(languageId, segment), _isPreviewing);
                cacheValues.XPathInitialized = true;
                return cacheValues.XPathValue;
            }
        }

        #region Classes

        private class CacheValues
        {
            private int? _languageId;
            private string _segment;

            public bool ObjectInitialized;
            public object ObjectValue;
            public bool XPathInitialized;
            public object XPathValue;

            private List<CacheValues> _values;

            // this is always invoked from within a lock, so does not require its own lock
            public CacheValues For(int? languageId, string segment)
            {
                if (languageId == null && segment == null)
                    return this;

                if (_values == null)
                    _values = new List<CacheValues>();

                var values = _values.FirstOrDefault(x => x._languageId == languageId && x._segment == segment);

                if (values == null)
                    _values.Add(values = new CacheValues { _languageId = languageId, _segment = segment });

                return values;
            }
        }

        private class PropertyValue
        {
            public int? LanguageId { get; set; }
            public string Segment { get; set; }
            public object SourceValue { get; set; }
            public bool InterInitialized { get; set; }
            public object InterValue { get; set; }
        }

        #endregion
    }
}
