using System;
using System.Xml.Serialization;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    class Property : PublishedPropertyBase
    {
        private readonly IFacadeAccessor _facadeAccessor;
        private readonly object _sourceValue;
        private readonly Guid _contentUid;
        private readonly bool _isPreviewing;
        private readonly bool _isMember;

        private readonly object _locko = new object();

        private bool _interInitialized;
        private object _interValue;
        private CacheValues _cacheValues;
        private string _valuesCacheKey;
        private string _recurseCacheKey;

        // initializes a published content property with no value
        public Property(PublishedPropertyType propertyType, IPublishedContent content, IFacadeAccessor facadeAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Content)
            : this(propertyType, content, null, facadeAccessor, referenceCacheLevel)
        { }

        // initializes a published content property with a value
        public Property(PublishedPropertyType propertyType, IPublishedContent content, object sourceValue, IFacadeAccessor facadeAccessor, PropertyCacheLevel referenceCacheLevel = PropertyCacheLevel.Content)
            : base(propertyType, referenceCacheLevel)
        {
            _sourceValue = sourceValue;
            _contentUid = content.Key;
            var inner = PublishedContent.UnwrapIPublishedContent(content);
            _isPreviewing = inner.IsPreviewing;
            _isMember = content.ContentType.ItemType == PublishedItemType.Member;
            _facadeAccessor = facadeAccessor;
        }

        // clone for previewing as draft a published content that is published and has no draft
        public Property(Property origin)
            : base(origin.PropertyType, origin.ReferenceCacheLevel)
        {
            _sourceValue = origin._sourceValue;
            _contentUid = origin._contentUid;
            _isPreviewing = true;
            _isMember = origin._isMember;
            _facadeAccessor = origin._facadeAccessor;
        }

        public override bool HasValue => _sourceValue != null
            && ((_sourceValue is string) == false || string.IsNullOrWhiteSpace((string)_sourceValue) == false);

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
            Facade facade;
            ICacheProvider cache;
            switch (cacheLevel)
            {
                case PropertyCacheLevel.None:
                    // never cache anything
                    cacheValues = new CacheValues();
                    break;
                case PropertyCacheLevel.Content:
                    // cache within the property object itself, ie within the content object
                    cacheValues = _cacheValues ?? (_cacheValues = new CacheValues());
                    break;
                case PropertyCacheLevel.Snapshot:
                    // cache within the snapshot cache, unless previewing, then use the facade or
                    // snapshot cache (if we don't want to pollute the snapshot cache with short-lived
                    // data) depending on settings
                    // for members, always cache in the facade cache - never pollute snapshot cache
                    facade = (Facade) _facadeAccessor.Facade;
                    cache = facade == null
                        ? null
                        : ((_isPreviewing == false || FacadeService.FullCacheWhenPreviewing) && (_isMember == false)
                            ? facade.SnapshotCache
                            : facade.FacadeCache);
                    cacheValues = GetCacheValues(cache);
                    break;
                case PropertyCacheLevel.Facade:
                    // cache within the facade cache
                    facade = (Facade) _facadeAccessor.Facade;
                    cache = facade?.FacadeCache;
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

            _interValue = PropertyType.ConvertSourceToInter(_sourceValue, _isPreviewing);
            _interInitialized = true;
            return _interValue;
        }

        public override object SourceValue => _sourceValue;

        public override object Value
        {
            get
            {
                lock (_locko)
                {
                    var cacheValues = GetCacheValues(PropertyType.CacheLevel);
                    if (cacheValues.ObjectInitialized) return cacheValues.ObjectValue;

                    // initial reference cache level always is .Content
                    cacheValues.ObjectValue = PropertyType.ConvertInterToObject(PropertyCacheLevel.Content, GetInterValue(), _isPreviewing);
                    cacheValues.ObjectInitialized = true;
                    return cacheValues.ObjectValue;
                }
            }
        }

        public override object XPathValue
        {
            get
            {
                lock (_locko)
                {
                    var cacheValues = GetCacheValues(PropertyType.CacheLevel);
                    if (cacheValues.XPathInitialized) return cacheValues.XPathValue;

                    // initial reference cache level always is .Content
                    cacheValues.XPathValue = PropertyType.ConvertInterToXPath(PropertyCacheLevel.Content, GetInterValue(), _isPreviewing);
                    cacheValues.XPathInitialized = true;
                    return cacheValues.XPathValue;
                }
            }
        }
    }
}
