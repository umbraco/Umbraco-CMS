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

        public ContentTypeCacheRefresher(AppCaches appCaches, IPublishedSnapshotService publishedSnapshotService, IdkMap idkMap)
            : base(appCaches)
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
            }

            if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
                // don't try to be clever - refresh all
                ContentCacheRefresher.RefreshContentTypes(AppCaches);

            if (payloads.Any(x => x.ItemType == typeof(IMediaType).Name))
                // don't try to be clever - refresh all
                MediaCacheRefresher.RefreshMediaTypes(AppCaches);

            if (payloads.Any(x => x.ItemType == typeof(IMemberType).Name))
                // don't try to be clever - refresh all
                MemberCacheRefresher.RefreshMemberTypes(AppCaches);

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
