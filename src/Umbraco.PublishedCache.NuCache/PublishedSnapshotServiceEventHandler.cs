using System;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache
{
    /// <summary>
    /// Subscribes to Umbraco events to ensure nucache remains consistent with the source data
    /// </summary>
    public class PublishedSnapshotServiceEventHandler : IDisposable
    {
        private readonly IRuntimeState _runtime;
        private bool _disposedValue;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly INuCacheContentService _publishedContentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshotServiceEventHandler"/> class.
        /// </summary>
        public PublishedSnapshotServiceEventHandler(
            IRuntimeState runtime,
            IPublishedSnapshotService publishedSnapshotService,
            INuCacheContentService publishedContentService)
        {
            _runtime = runtime;
            _publishedSnapshotService = publishedSnapshotService;
            _publishedContentService = publishedContentService;
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

            // we always want to handle repository events, configured or not
            // assuming no repository event will trigger before the whole db is ready
            // (ideally we'd have Upgrading.App vs Upgrading.Data application states...)
            InitializeRepositoryEvents();

            return true;
        }

        private void InitializeRepositoryEvents()
        {
            // TODO: The reason these events are in the repository is for legacy, the events should exist at the service
            // level now since we can fire these events within the transaction... so move the events to service level

            // plug repository event handlers
            // these trigger within the transaction to ensure consistency
            // and are used to maintain the central, database-level XML cache
            DocumentRepository.ScopeEntityRemove += OnContentRemovingEntity;
            DocumentRepository.ScopedEntityRefresh += DocumentRepository_ScopedEntityRefresh;
            MediaRepository.ScopeEntityRemove += OnMediaRemovingEntity;
            MediaRepository.ScopedEntityRefresh += MediaRepository_ScopedEntityRefresh;
            MemberRepository.ScopeEntityRemove += OnMemberRemovingEntity;
            MemberRepository.ScopedEntityRefresh += MemberRepository_ScopedEntityRefresh;

            // plug
            ContentTypeService.ScopedRefreshedEntity += OnContentTypeRefreshedEntity;
            MediaTypeService.ScopedRefreshedEntity += OnMediaTypeRefreshedEntity;
            MemberTypeService.ScopedRefreshedEntity += OnMemberTypeRefreshedEntity;

            // TODO: This should be a cache refresher call!
            LocalizationService.SavedLanguage += OnLanguageSaved;
        }

        private void TearDownRepositoryEvents()
        {
            DocumentRepository.ScopeEntityRemove -= OnContentRemovingEntity;
            DocumentRepository.ScopedEntityRefresh -= DocumentRepository_ScopedEntityRefresh;
            MediaRepository.ScopeEntityRemove -= OnMediaRemovingEntity;
            MediaRepository.ScopedEntityRefresh -= MediaRepository_ScopedEntityRefresh;
            MemberRepository.ScopeEntityRemove -= OnMemberRemovingEntity;
            MemberRepository.ScopedEntityRefresh -= MemberRepository_ScopedEntityRefresh;
            ContentTypeService.ScopedRefreshedEntity -= OnContentTypeRefreshedEntity;
            MediaTypeService.ScopedRefreshedEntity -= OnMediaTypeRefreshedEntity;
            MemberTypeService.ScopedRefreshedEntity -= OnMemberTypeRefreshedEntity;
            LocalizationService.SavedLanguage -= OnLanguageSaved; // TODO: Shouldn't this be a cache refresher event?
        }

        // note: if the service is not ready, ie _isReady is false, then we still handle repository events,
        // because we can, we do not need a working published snapshot to do it - the only reason why it could cause an
        // issue is if the database table is not ready, but that should be prevented by migrations.

        // we need them to be "repository" events ie to trigger from within the repository transaction,
        // because they need to be consistent with the content that is being refreshed/removed - and that
        // should be guaranteed by a DB transaction
        private void OnContentRemovingEntity(DocumentRepository sender, DocumentRepository.ScopedEntityEventArgs args)
            => _publishedContentService.DeleteContentItem(args.Entity);

        private void OnMediaRemovingEntity(MediaRepository sender, MediaRepository.ScopedEntityEventArgs args)
            => _publishedContentService.DeleteContentItem(args.Entity);

        private void OnMemberRemovingEntity(MemberRepository sender, MemberRepository.ScopedEntityEventArgs args)
            => _publishedContentService.DeleteContentItem(args.Entity);

        private void MemberRepository_ScopedEntityRefresh(MemberRepository sender, ContentRepositoryBase<int, IMember, MemberRepository>.ScopedEntityEventArgs e)
            => _publishedContentService.RefreshEntity(e.Entity);

        private void MediaRepository_ScopedEntityRefresh(MediaRepository sender, ContentRepositoryBase<int, IMedia, MediaRepository>.ScopedEntityEventArgs e)
            => _publishedContentService.RefreshEntity(e.Entity);

        private void DocumentRepository_ScopedEntityRefresh(DocumentRepository sender, ContentRepositoryBase<int, IContent, DocumentRepository>.ScopedEntityEventArgs e)
            => _publishedContentService.RefreshContent(e.Entity);

        private void OnContentTypeRefreshedEntity(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var contentTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (contentTypeIds.Any())
            {
                _publishedSnapshotService.Rebuild(contentTypeIds: contentTypeIds);
            }
        }

        private void OnMediaTypeRefreshedEntity(IMediaTypeService sender, ContentTypeChange<IMediaType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var mediaTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (mediaTypeIds.Any())
            {
                _publishedSnapshotService.Rebuild(mediaTypeIds: mediaTypeIds);
            }
        }

        private void OnMemberTypeRefreshedEntity(IMemberTypeService sender, ContentTypeChange<IMemberType>.EventArgs args)
        {
            const ContentTypeChangeTypes types // only for those that have been refreshed
                = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
            var memberTypeIds = args.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id).ToArray();
            if (memberTypeIds.Any())
            {
                _publishedSnapshotService.Rebuild(memberTypeIds: memberTypeIds);
            }
        }

        /// <summary>
        /// If a <see cref="ILanguage"/> is ever saved with a different culture, we need to rebuild all of the content nucache database table
        /// </summary>
        private void OnLanguageSaved(ILocalizationService sender, SaveEventArgs<ILanguage> e)
        {
            // culture changed on an existing language
            var cultureChanged = e.SavedEntities.Any(x => !x.WasPropertyDirty(nameof(ILanguage.Id)) && x.WasPropertyDirty(nameof(ILanguage.IsoCode)));
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
                if (disposing)
                {
                    TearDownRepositoryEvents();
                }

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
