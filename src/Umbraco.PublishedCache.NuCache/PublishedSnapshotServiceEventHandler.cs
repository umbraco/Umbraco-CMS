using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Notifications;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache
{
    /// <summary>
    /// Subscribes to Umbraco events to ensure nucache remains consistent with the source data
    /// </summary>
    public class PublishedSnapshotServiceEventHandler :
        IDisposable,
        INotificationHandler<LanguageSavedNotification>,
        INotificationHandler<ContentDeletingNotification>,
        INotificationHandler<MediaDeletingNotification>,
        INotificationHandler<MemberDeletingNotification>,
        INotificationHandler<ContentEmptyingRecycleBinNotification>,
        INotificationHandler<MediaEmptyingRecycleBinNotification>,
        INotificationHandler<ContentRefreshNotification>,
        INotificationHandler<MediaRefreshNotification>,
        INotificationHandler<MemberRefreshNotification>,
        INotificationHandler<ContentTypeRefreshedNotification>,
        INotificationHandler<MediaTypeRefreshedNotification>,
        INotificationHandler<MemberTypeRefreshedNotification>
    {
        private readonly IRuntimeState _runtime;
        private bool _disposedValue;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly INuCacheContentService _publishedContentService;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshotServiceEventHandler"/> class.
        /// </summary>
        public PublishedSnapshotServiceEventHandler(
            IRuntimeState runtime,
            IPublishedSnapshotService publishedSnapshotService,
            INuCacheContentService publishedContentService,
            IContentService contentService,
            IMediaService mediaService)
        {
            _runtime = runtime;
            _publishedSnapshotService = publishedSnapshotService;
            _publishedContentService = publishedContentService;
            _contentService = contentService;
            _mediaService = mediaService;
        }

        /// <summary>
        /// Binds to the Umbraco events
        /// </summary>
        /// <returns>Returns true if binding occurred</returns>
        public bool Initialize()
        {
            // however, the cache is NOT available until we are configured, because loading
            // content (and content types) from database cannot be consistent (see notes in "Handle
            // Notifications" region), so
            // - notifications will be ignored
            // - trying to obtain a published snapshot from the service will throw
            if (_runtime.Level != RuntimeLevel.Run)
            {
                return false;
            }

            return true;
        }

        // note: if the service is not ready, ie _isReady is false, then we still handle repository events,
        // because we can, we do not need a working published snapshot to do it - the only reason why it could cause an
        // issue is if the database table is not ready, but that should be prevented by migrations.

        // we need them to be "repository" events ie to trigger from within the repository transaction,
        // because they need to be consistent with the content that is being refreshed/removed - and that
        // should be guaranteed by a DB transaction
        public void Handle(ContentDeletingNotification notification) => HandleContentEntitiesDeleted(notification.DeletedEntities);

        public void Handle(ContentEmptyingRecycleBinNotification notification) => HandleContentEntitiesDeleted(notification.DeletedEntities);

        public void Handle(MediaDeletingNotification notification) => HandleMediaEntitiesDeleted(notification.DeletedEntities);

        public void Handle(MediaEmptyingRecycleBinNotification notification) => HandleMediaEntitiesDeleted(notification.DeletedEntities);

        public void Handle(MemberDeletingNotification notification) => _publishedContentService.DeleteContentItems(notification.DeletedEntities);

        private void HandleContentEntitiesDeleted(IEnumerable<IContentBase> entities)
        {
            var entitiesToDelete = new HashSet<IContentBase>();
            foreach (IContentBase entity in entities)
            {
                IEnumerable<IContent> descendants =
                    _contentService.GetPagedDescendants(entity.Id, 0, int.MaxValue, out long totalRecords);
                entitiesToDelete.Add(entity);
                foreach (IContent descendant in descendants)
                {
                    entitiesToDelete.Add(descendant);
                }
            }
            _publishedContentService.DeleteContentItems(entitiesToDelete);
        }

        private void HandleMediaEntitiesDeleted(IEnumerable<IContentBase> entities)
        {
            var entitiesToDelete = new HashSet<IContentBase>();
            foreach (IContentBase entity in entities)
            {
                IEnumerable<IMedia> descendants = _mediaService.GetPagedDescendants(entity.Id, 0, int.MaxValue, out long totalRecords);
                entitiesToDelete.Add(entity);
                foreach (IMedia descendant in descendants)
                {
                    entitiesToDelete.Add(descendant);
                }
            }
            _publishedContentService.DeleteContentItems(entitiesToDelete);
        }

        public void Handle(MemberRefreshNotification notification) => _publishedContentService.RefreshEntity(notification.Entity);

        public void Handle(MediaRefreshNotification notification) => _publishedContentService.RefreshEntity(notification.Entity);

        public void Handle(ContentRefreshNotification notification) => _publishedContentService.RefreshContent(notification.Entity);

        public void Handle(ContentTypeRefreshedNotification notification)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var contentTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (contentTypeIds.Any())
            {
                _publishedSnapshotService.Rebuild(contentTypeIds: contentTypeIds);
            }
        }

        public void Handle(MediaTypeRefreshedNotification notification)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var mediaTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (mediaTypeIds.Any())
            {
                _publishedSnapshotService.Rebuild(mediaTypeIds: mediaTypeIds);
            }
        }

        public void Handle(MemberTypeRefreshedNotification notification)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var memberTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (memberTypeIds.Any())
            {
                _publishedSnapshotService.Rebuild(memberTypeIds: memberTypeIds);
            }
        }

        // TODO: This should be a cache refresher call!
        /// <summary>
        /// If a <see cref="ILanguage"/> is ever saved with a different culture, we need to rebuild all of the content nucache database table
        /// </summary>
        public void Handle(LanguageSavedNotification notification)
        {
            // culture changed on an existing language
            var cultureChanged = notification.SavedEntities.Any(x => !x.WasPropertyDirty(nameof(ILanguage.Id)) && x.WasPropertyDirty(nameof(ILanguage.IsoCode)));
            if (cultureChanged)
            {
                // Rebuild all content for all content types
                _publishedSnapshotService.Rebuild(contentTypeIds: Array.Empty<int>());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
