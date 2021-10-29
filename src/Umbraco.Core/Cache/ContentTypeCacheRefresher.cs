using System;
using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    /// <inheritdoc />
    public sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresherNotification, ContentTypeCacheRefresher.JsonPayload>
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
        private readonly IIdKeyMap _idKeyMap;
        public static readonly Guid UniqueId = Guid.Parse("6902E22C-9C10-483C-91F3-66B7CAE9E2F5");

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeCacheRefresher"/> class.
        /// </summary>
        /// <param name="appCaches">The <see cref="AppCaches"/> to use</param>
        /// <param name="serializer">The <see cref="IJsonSerializer"/> to use</param>
        /// <param name="publishedSnapshotService">The <see cref="IPublishedSnapshotService"/> to use</param>
        /// <param name="publishedModelFactory">The <see cref="IPublishedModelFactory"/> to use</param>
        /// <param name="idKeyMap">The <see cref="IIdKeyMap"/> to use</param>
        /// <param name="contentTypeCommonRepository">The <see cref="IContentTypeCommonRepository"/> to use</param>
        /// <param name="eventAggregator">The <see cref="IEventAggregator"/> to use</param>
        /// <param name="factory">The <see cref="ICacheRefresherNotificationFactory"/> to use</param>
        public ContentTypeCacheRefresher(
            AppCaches appCaches,
            IJsonSerializer serializer,
            IPublishedSnapshotService publishedSnapshotService,
            IPublishedModelFactory publishedModelFactory,
            IIdKeyMap idKeyMap,
            IContentTypeCommonRepository contentTypeCommonRepository,
            IEventAggregator eventAggregator,
            ICacheRefresherNotificationFactory factory)
            : base(appCaches, serializer, eventAggregator, factory)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _publishedModelFactory = publishedModelFactory;
            _idKeyMap = idKeyMap;
            _contentTypeCommonRepository = contentTypeCommonRepository;
        }

        #region Define

        /// <inheritdoc/>
        public override Guid RefresherUniqueId => UniqueId;

        /// <inheritdoc/>
        public override string Name => "Content Type Cache Refresher";

        #endregion

        #region Refresher

        /// <inheritdoc/>
        public override void Refresh(JsonPayload[] payloads)
        {
            // TODO: refactor
            // we should NOT directly clear caches here, but instead ask whatever class
            // is managing the cache to please clear that cache properly
            _contentTypeCommonRepository.ClearCache(); // always

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
                _idKeyMap.ClearCache(id);
            }

            if (payloads.Any(x => x.ItemType == typeof(IContentType).Name))
            {
                // don't try to be clever - refresh all
                ContentCacheRefresher.RefreshContentTypes(AppCaches);
            }

            if (payloads.Any(x => x.ItemType == typeof(IMediaType).Name))
            {
                // don't try to be clever - refresh all
                MediaCacheRefresher.RefreshMediaTypes(AppCaches);
            }

            if (payloads.Any(x => x.ItemType == typeof(IMemberType).Name))
            {
                // don't try to be clever - refresh all
                MemberCacheRefresher.RefreshMemberTypes(AppCaches);
            }

            // refresh the models and cache
            _publishedModelFactory.WithSafeLiveFactoryReset(() =>
                _publishedSnapshotService.Notify(payloads));

            // now we can trigger the event
            base.Refresh(payloads);
        }


        /// <inheritdoc/>
        public override void RefreshAll()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Refresh(int id)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Refresh(Guid id)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Remove(int id)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Json

        public class JsonPayload
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="JsonPayload"/> class.
            /// </summary>
            /// <param name="itemType">The itemType string</param>
            /// <param name="id">The id</param>
            /// <param name="changeTypes">The <see cref="ContentTypeChangeTypes"/> to use</param>
            public JsonPayload(string itemType, int id, ContentTypeChangeTypes changeTypes)
            {
                ItemType = itemType;
                Id = id;
                ChangeTypes = changeTypes;
            }

            /// <summary>
            /// Gets the Id of the Payload
            /// </summary>
            public string ItemType { get; }

            /// <summary>
            /// Gets the Key of the Payload
            /// </summary>
            public int Id { get; }

            /// <summary>
            /// Gets the <see cref="ContentTypeChangeTypes"/> of the Payload/>
            /// </summary>
            public ContentTypeChangeTypes ChangeTypes { get; }
        }

        #endregion
    }
}
