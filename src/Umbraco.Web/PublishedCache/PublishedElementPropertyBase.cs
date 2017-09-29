using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache
{
    internal class PublishedElementPropertyBase : PublishedPropertyBase
    {
        private readonly object _locko = new object();
        private readonly object _sourceValue;
        private readonly IFacadeAccessor _facadeAccessor;

        protected readonly IPublishedElement Element;
        protected readonly bool IsPreviewing;
        protected readonly bool IsMember;

        private bool _interInitialized;
        private object _interValue;
        private CacheValues _cacheValues;
        private string _valuesCacheKey;

        // define constant - determines whether to use cache when previewing
        // to store eg routes, property converted values, anything - caching
        // means faster execution, but uses memory - not sure if we want it
        // so making it configureable.
        private const bool FullCacheWhenPreviewing = true;

        public PublishedElementPropertyBase(PublishedPropertyType propertyType, IPublishedElement element, bool previewing, PropertyCacheLevel referenceCacheLevel, object sourceValue = null, IFacadeAccessor facadeAccessor = null)
            : base(propertyType, referenceCacheLevel)
        {
            _sourceValue = sourceValue;
            _facadeAccessor = facadeAccessor;
            Element = element;
            IsPreviewing = previewing;
            IsMember = propertyType.ContentType.ItemType == PublishedItemType.Member;
        }

        public override bool HasValue
            => _sourceValue != null && (!(_sourceValue is string s) || !string.IsNullOrWhiteSpace(s));

        // used to cache the CacheValues of this property
        // ReSharper disable InconsistentlySynchronizedField
        internal string ValuesCacheKey => _valuesCacheKey
            ?? (_valuesCacheKey = PropertyCacheValues(Element.Key, PropertyTypeAlias, IsPreviewing));
        // ReSharper restore InconsistentlySynchronizedField

        protected class CacheValues
        {
            public bool ObjectInitialized;
            public object ObjectValue;
            public bool XPathInitialized;
            public object XPathValue;
        }

        public static string PropertyCacheValues(Guid contentUid, string typeAlias, bool previewing)
        {
            return "Facade.Property.CacheValues[" + (previewing ? "D:" : "P:") + contentUid + ":" + typeAlias + "]";
        }

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
            // currently (reference) caching at facade, property specifies
            // snapshot, ok to use content. OTOH, currently caching at snapshot,
            // property specifies facade, need to use facade.
            //
            if (PropertyType.CacheLevel > ReferenceCacheLevel || PropertyType.CacheLevel == PropertyCacheLevel.None)
            {
                cacheLevel = PropertyType.CacheLevel;
                referenceCacheLevel = cacheLevel;
            }
            else
            {
                cacheLevel = PropertyCacheLevel.Content;
                referenceCacheLevel = ReferenceCacheLevel;
            }
        }

        private ICacheProvider GetSnapshotCache()
        {
            // cache within the snapshot cache, unless previewing, then use the facade or
            // snapshot cache (if we don't want to pollute the snapshot cache with short-lived
            // data) depending on settings
            // for members, always cache in the facade cache - never pollute snapshot cache
            var facade = _facadeAccessor?.Facade;
            if (facade == null) return null;
            return (IsPreviewing == false || FullCacheWhenPreviewing) && IsMember == false
                ? facade.SnapshotCache
                : facade.FacadeCache;
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
                case PropertyCacheLevel.Content:
                    // cache within the property object itself, ie within the content object
                    cacheValues = _cacheValues ?? (_cacheValues = new CacheValues());
                    break;
                case PropertyCacheLevel.Snapshot:
                    // cache within the snapshot cache, depending...
                    var snapshotCache = GetSnapshotCache();
                    cacheValues = (CacheValues) snapshotCache?.GetCacheItem(ValuesCacheKey, () => new CacheValues()) ?? new CacheValues();
                    break;
                case PropertyCacheLevel.Facade:
                    // cache within the facade cache
                    var facadeCache = _facadeAccessor?.Facade?.FacadeCache;
                    cacheValues = (CacheValues) facadeCache?.GetCacheItem(ValuesCacheKey, () => new CacheValues()) ?? new CacheValues();
                    break;
                default:
                    throw new InvalidOperationException("Invalid cache level.");
            }
            return cacheValues;
        }

        private object GetInterValue()
        {
            if (_interInitialized) return _interValue;

            _interValue = PropertyType.ConvertSourceToInter(Element, _sourceValue, IsPreviewing);
            _interInitialized = true;
            return _interValue;
        }

        public override object SourceValue => _sourceValue;

        public override object Value
        {
            get
            {
                GetCacheLevels(out var cacheLevel, out var referenceCacheLevel);

                lock (_locko)
                {
                    var cacheValues = GetCacheValues(cacheLevel);
                    if (cacheValues.ObjectInitialized) return cacheValues.ObjectValue;
                    cacheValues.ObjectValue = PropertyType.ConvertInterToObject(Element, referenceCacheLevel, GetInterValue(), IsPreviewing);
                    cacheValues.ObjectInitialized = true;
                    return cacheValues.ObjectValue;
                }
            }
        }

        public override object XPathValue
        {
            get
            {
                GetCacheLevels(out var cacheLevel, out var referenceCacheLevel);

                lock (_locko)
                {
                    var cacheValues = GetCacheValues(cacheLevel);
                    if (cacheValues.XPathInitialized) return cacheValues.XPathValue;
                    cacheValues.XPathValue = PropertyType.ConvertInterToXPath(Element, referenceCacheLevel, GetInterValue(), IsPreviewing);
                    cacheValues.XPathInitialized = true;
                    return cacheValues.XPathValue;
                }
            }
        }
    }
}
