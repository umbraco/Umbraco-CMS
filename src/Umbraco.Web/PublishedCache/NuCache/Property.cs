using System;
using System.Xml.Serialization;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    internal class Property : PublishedPropertyBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly object _sourceValue;
        private readonly Guid _contentUid;
        private readonly bool _isPreviewing;
        private readonly bool _isMember;
        private readonly IPublishedContent _content;

        private readonly object _locko = new object();

        private bool _interInitialized;
        private object _interValue;
        private CacheValues _cacheValues;
        private string _valuesCacheKey;
        private string _recurseCacheKey;

        // initializes a published content property with no value
        public Property(PublishedPropertyType propertyType, PublishedContent content, IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
            : this(propertyType, content, null, publishedSnapshotAccessor, referenceCacheLevel)
        { }

        // initializes a published content property with a value
        public Property(PublishedPropertyType propertyType, PublishedContent content, object sourceValue, IPublishedSnapshotAccessor publishedSnapshotAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Element)
            : base(propertyType, referenceCacheLevel)
        {
            _sourceValue = sourceValue;
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
            _contentUid = origin._contentUid;
            _content = content;
            _isPreviewing = true;
            _isMember = origin._isMember;
            _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
        }

        public override bool HasValue(int? languageId = null, string segment = null) => _sourceValue != null
            && (!(_sourceValue is string) || string.IsNullOrWhiteSpace((string) _sourceValue) == false);

        private class CacheValues
        {
            public bool ObjectInitialized;
            public object ObjectValue;
            public bool XPathInitialized;
            public object XPathValue;
        }

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

        private object GetInterValue()
        {
            if (_interInitialized) return _interValue;

            _interValue = PropertyType.ConvertSourceToInter(_content, _sourceValue, _isPreviewing);
            _interInitialized = true;
            return _interValue;
        }

        public override object GetSourceValue(int? languageId = null, string segment = null) => _sourceValue;

        public override object GetValue(int? languageId = null, string segment = null)
        {
            lock (_locko)
            {
                var cacheValues = GetCacheValues(PropertyType.CacheLevel);
                if (cacheValues.ObjectInitialized) return cacheValues.ObjectValue;

                // initial reference cache level always is .Content
                cacheValues.ObjectValue = PropertyType.ConvertInterToObject(_content, PropertyCacheLevel.Element, GetInterValue(), _isPreviewing);
                cacheValues.ObjectInitialized = true;
                return cacheValues.ObjectValue;
            }
        }

        public override object GetXPathValue(int? languageId = null, string segment = null)
        {
            lock (_locko)
            {
                var cacheValues = GetCacheValues(PropertyType.CacheLevel);
                if (cacheValues.XPathInitialized) return cacheValues.XPathValue;

                // initial reference cache level always is .Content
                cacheValues.XPathValue = PropertyType.ConvertInterToXPath(_content, PropertyCacheLevel.Element, GetInterValue(), _isPreviewing);
                cacheValues.XPathInitialized = true;
                return cacheValues.XPathValue;
            }
        }
    }
}
