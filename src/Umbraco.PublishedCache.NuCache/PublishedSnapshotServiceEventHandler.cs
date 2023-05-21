using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

/// <summary>
///     Subscribes to Umbraco events to ensure nucache remains consistent with the source data
/// </summary>
public class PublishedSnapshotServiceEventHandler :
    INotificationHandler<LanguageSavedNotification>,
    INotificationHandler<MemberDeletingNotification>,
    INotificationHandler<ContentRefreshNotification>,
    INotificationHandler<MediaRefreshNotification>,
    INotificationHandler<MemberRefreshNotification>,
    INotificationHandler<ContentTypeRefreshedNotification>,
    INotificationHandler<MediaTypeRefreshedNotification>,
    INotificationHandler<MemberTypeRefreshedNotification>,
    INotificationHandler<ScopedEntityRemoveNotification>
{
    private readonly INuCacheContentService _publishedContentService;
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedSnapshotServiceEventHandler" /> class.
    /// </summary>
    public PublishedSnapshotServiceEventHandler(
        IPublishedSnapshotService publishedSnapshotService,
        INuCacheContentService publishedContentService)
    {
        _publishedSnapshotService = publishedSnapshotService;
        _publishedContentService = publishedContentService;
    }

    [Obsolete("Please use alternative constructor.")]
    public PublishedSnapshotServiceEventHandler(
        IRuntimeState runtime,
        IPublishedSnapshotService publishedSnapshotService,
        INuCacheContentService publishedContentService,
        IContentService contentService,
        IMediaService mediaService)
        : this(publishedSnapshotService, publishedContentService)
    {
    }

    public void Handle(ContentRefreshNotification notification) =>
        _publishedContentService.RefreshContent(notification.Entity);

    public void Handle(ContentTypeRefreshedNotification notification)
    {
        const ContentTypeChangeTypes types // only for those that have been refreshed
            = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
        var contentTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id)
            .ToArray();
        if (contentTypeIds.Any())
        {
            _publishedSnapshotService.Rebuild(contentTypeIds);
        }
    }

    // TODO: This should be a cache refresher call!

    /// <summary>
    ///     If a <see cref="ILanguage" /> is ever saved with a different culture, we need to rebuild all of the content nucache
    ///     database table
    /// </summary>
    public void Handle(LanguageSavedNotification notification)
    {
        // culture changed on an existing language
        var cultureChanged = notification.SavedEntities.Any(x =>
            !x.WasPropertyDirty(nameof(ILanguage.Id)) && x.WasPropertyDirty(nameof(ILanguage.IsoCode)));
        if (cultureChanged)
        {
            // Rebuild all content for all content types
            _publishedSnapshotService.Rebuild(Array.Empty<int>());
        }
    }

    public void Handle(MediaRefreshNotification notification) =>
        _publishedContentService.RefreshMedia(notification.Entity);

    public void Handle(MediaTypeRefreshedNotification notification)
    {
        const ContentTypeChangeTypes types // only for those that have been refreshed
            = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
        var mediaTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id)
            .ToArray();
        if (mediaTypeIds.Any())
        {
            _publishedSnapshotService.Rebuild(mediaTypeIds: mediaTypeIds);
        }
    }

    public void Handle(MemberDeletingNotification notification) =>
        _publishedContentService.DeleteContentItems(notification.DeletedEntities);

    public void Handle(MemberRefreshNotification notification) =>
        _publishedContentService.RefreshMember(notification.Entity);

    public void Handle(MemberTypeRefreshedNotification notification)
    {
        const ContentTypeChangeTypes types // only for those that have been refreshed
            = ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther;
        var memberTypeIds = notification.Changes.Where(x => x.ChangeTypes.HasTypesAny(types)).Select(x => x.Item.Id)
            .ToArray();
        if (memberTypeIds.Any())
        {
            _publishedSnapshotService.Rebuild(memberTypeIds: memberTypeIds);
        }
    }

    // note: if the service is not ready, ie _isReady is false, then we still handle repository events,
    // because we can, we do not need a working published snapshot to do it - the only reason why it could cause an
    // issue is if the database table is not ready, but that should be prevented by migrations.

    // we need them to be "repository" events ie to trigger from within the repository transaction,
    // because they need to be consistent with the content that is being refreshed/removed - and that
    // should be guaranteed by a DB transaction
    public void Handle(ScopedEntityRemoveNotification notification) =>
        _publishedContentService.DeleteContentItem(notification.Entity);
}
