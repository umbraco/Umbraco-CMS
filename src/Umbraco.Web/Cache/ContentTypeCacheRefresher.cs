using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Cache
{
    public sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresher, ContentTypeCacheRefresher.JsonPayload>
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
        private readonly IContentTypeService _contentTypeService;
        private readonly IdkMap _idkMap;

        public ContentTypeCacheRefresher(AppCaches appCaches, IPublishedSnapshotService publishedSnapshotService, IPublishedModelFactory publishedModelFactory, IdkMap idkMap, IContentTypeCommonRepository contentTypeCommonRepository, IContentTypeService contentTypeService)
            : base(appCaches)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _publishedModelFactory = publishedModelFactory;
            _idkMap = idkMap;
            _contentTypeCommonRepository = contentTypeCommonRepository;
            _contentTypeService = contentTypeService;
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

            _contentTypeCommonRepository.ClearCache(); // always

            // We need to special handle the IContentType if modelsbuilder is in live mode, because all models are updated when a IContentType is changed, we need to clear all from cache also.
            if (_publishedModelFactory is ILivePublishedModelFactory && payloads.Any(x => x.ItemType == typeof(IContentType).Name))
            {
                //This is super nasty, and we need to figure out a better way to to this
                //Ensure all doc type ids is part of the payload
                var missingPayloads = GetMissingContentTypePayloads(payloads);

                payloads = payloads.Union(missingPayloads).ToArray();
            }

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

            // we have to refresh models before we notify the published snapshot
            // service of changes, else factories may try to rebuild models while
            // we are using the database to load content into caches

            _publishedModelFactory.WithSafeLiveFactory(() =>
                _publishedSnapshotService.Notify(payloads));

            // now we can trigger the event
            base.Refresh(payloads);
        }

        private IEnumerable<JsonPayload> GetMissingContentTypePayloads(JsonPayload[] payloads)
        {
            var existingPayloadIds = new HashSet<int>(payloads.Select(x => x.Id));
            var contentTypeIds = _contentTypeService.GetAll().Select(x => x.Id).ToArray();

            foreach (var contentTypeId in contentTypeIds)
            {
                if (!existingPayloadIds.Contains(contentTypeId))
                {
                    yield return new JsonPayload(typeof(IContentType).Name, contentTypeId, ContentTypeChangeTypes.RefreshOther);
                }
            }
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
