using System;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Cache
{
    public sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresher, ContentTypeCacheRefresher.JsonPayload>
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IdkMap _idkMap;

        public ContentTypeCacheRefresher(CacheHelper cacheHelper, IPublishedSnapshotService publishedSnapshotService, IdkMap idkMap)
            : base(cacheHelper)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _idkMap = idkMap;
        }

        #region Define

        protected override ContentTypeCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("6902E22C-9C10-483C-91F3-66B7CAE9E2F5");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Content Type Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(JsonPayload[] payloads)
        {
            // TODO: refactor
            // we should NOT directly clear caches here, but instead ask whatever class
            // is managing the cache to please clear that cache properly

            if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
            {
                ClearAllIsolatedCacheByEntityType<IContent>();
                ClearAllIsolatedCacheByEntityType<IContentType>();
            }

            if (payloads.Any(x => x.ItemType == typeof(IMediaType).Name))
            {
                ClearAllIsolatedCacheByEntityType<IMedia>();
                ClearAllIsolatedCacheByEntityType<IMediaType>();
            }

            if (payloads.Any(x => x.ItemType == typeof(IMemberType).Name))
            {
                ClearAllIsolatedCacheByEntityType<IMember>();
                ClearAllIsolatedCacheByEntityType<IMemberType>();
            }

            foreach (var id in payloads.Select(x => x.Id))
            {
                _idkMap.ClearCache(id);
                ClearLegacyCaches(id);
            }

            if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
                // don't try to be clever - refresh all
                ContentCacheRefresher.RefreshContentTypes(CacheHelper);

            if (payloads.Any(x => x.ItemType == typeof(IMediaType).Name))
                // don't try to be clever - refresh all
                MediaCacheRefresher.RefreshMediaTypes(CacheHelper);

            if (payloads.Any(x => x.ItemType == typeof(IMemberType).Name))
                // don't try to be clever - refresh all
                MemberCacheRefresher.RefreshMemberTypes(CacheHelper);

            // notify
            _publishedSnapshotService.Notify(payloads);

            // now we can trigger the event
            base.Refresh(payloads);
        }

        public override void RefreshAll()
        {
            throw new NotSupportedException();
        }

        public override void Refresh(int id)
        {
            throw new NotSupportedException();
        }

        public override void Refresh(Guid id)
        {
            throw new NotSupportedException();
        }

        public override void Remove(int id)
        {
            throw new NotSupportedException();
        }

        private void ClearLegacyCaches(int contentTypeId /*, string contentTypeAlias, IEnumerable<int> propertyTypeIds*/)
        {
            // legacy umbraco.cms.businesslogic.ContentType

            // TODO - get rid of all this mess

            // clears the cache for each property type associated with the content type
            // see src/umbraco.cms/businesslogic/propertytype/propertytype.cs
            // that cache is disabled because we could not clear it properly
            //foreach (var pid in propertyTypeIds)
            //    ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(CacheKeys.PropertyTypeCacheKey + pid);

            // clears the cache associated with the content type itself
            CacheHelper.RuntimeCache.ClearCacheItem(CacheKeys.ContentTypeCacheKey + contentTypeId);

            // clears the cache associated with the content type properties collection
            CacheHelper.RuntimeCache.ClearCacheItem(CacheKeys.ContentTypePropertiesCacheKey + contentTypeId);

            // clears the dictionary object cache of the legacy ContentType
            // see src/umbraco.cms/businesslogic/ContentType.cs
            // that cache is disabled because we could not clear it properly
            //global::umbraco.cms.businesslogic.ContentType.RemoveFromDataTypeCache(contentTypeAlias);
        }

        #endregion

        #region Json

        public class JsonPayload
        {
            public JsonPayload(string itemType, int id, ContentTypeChangeTypes changeTypes)
            {
                ItemType = itemType;
                Id = id;
                ChangeTypes = changeTypes;
            }

            public string ItemType { get; }

            public int Id { get; }

            public ContentTypeChangeTypes ChangeTypes { get; }
        }

        #endregion
    }
}
