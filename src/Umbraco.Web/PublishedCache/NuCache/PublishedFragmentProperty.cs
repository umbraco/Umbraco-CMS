using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class PublishedFragmentProperty : PublishedCache.PublishedFragmentProperty
    {
        private readonly IFacadeAccessor _facadeAccessor;
        private string _valuesCacheKey;

        // initializes a published item property
        public PublishedFragmentProperty(IFacadeAccessor facadeAccessor, PublishedPropertyType propertyType, Guid itemKey, bool previewing, PropertyCacheLevel referenceCacheLevel, object sourceValue = null)
            : base(propertyType, itemKey, previewing, referenceCacheLevel, sourceValue)
        {
            _facadeAccessor = facadeAccessor;
        }

        // used to cache the CacheValues of this property
        internal string ValuesCacheKey => _valuesCacheKey
            ?? (_valuesCacheKey = CacheKeys.PropertyCacheValues(ItemUid, PropertyTypeAlias, IsPreviewing));

        protected override CacheValues GetSnapshotCacheValues()
        {
            // cache within the snapshot cache, unless previewing, then use the facade or
            // snapshot cache (if we don't want to pollute the snapshot cache with short-lived
            // data) depending on settings
            // for members, always cache in the facade cache - never pollute snapshot cache
            var facade = (Facade)_facadeAccessor.Facade;
            var cache = facade == null
                ? null
                : ((IsPreviewing == false || FacadeService.FullCacheWhenPreviewing) && (IsMember == false)
                    ? facade.SnapshotCache
                    : facade.FacadeCache);
            return  GetCacheValues(cache);
        }

        protected override CacheValues GetFacadeCacheValues()
        {
            // cache within the facade cache
            var facade = (Facade) _facadeAccessor.Facade;
            var cache = facade?.FacadeCache;
            return GetCacheValues(cache);
        }

        private CacheValues GetCacheValues(ICacheProvider cache)
        {
            // no cache, don't cache
            return cache == null
                ? new CacheValues()
                : (CacheValues) cache.GetCacheItem(ValuesCacheKey, () => new CacheValues());
        }
    }
}
