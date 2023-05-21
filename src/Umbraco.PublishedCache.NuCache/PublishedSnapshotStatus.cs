using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

/// <summary>
///     Generates a status report for <see cref="PublishedSnapshotService" />
/// </summary>
internal class PublishedSnapshotStatus : IPublishedSnapshotStatus
{
    private readonly INuCacheContentService _publishedContentService;
    private readonly PublishedSnapshotService? _service;

    public PublishedSnapshotStatus(IPublishedSnapshotService? service, INuCacheContentService publishedContentService)
    {
        _service = service as PublishedSnapshotService;
        _publishedContentService = publishedContentService;
    }

    /// <inheritdoc />
    public virtual string StatusUrl => "views/dashboard/settings/publishedsnapshotcache.html";

    /// <inheritdoc />
    public string GetStatus()
    {
        if (_service == null)
        {
            return
                $"The current {typeof(IPublishedSnapshotService)} is not the default type. A status cannot be determined.";
        }

        // TODO: This should be private
        _service.EnsureCaches();

        var dbCacheIsOk = _publishedContentService.VerifyContentDbCache()
                          && _publishedContentService.VerifyMediaDbCache()
                          && _publishedContentService.VerifyMemberDbCache()
            ? "ok"
            : "NOT ok (rebuild?)";

        ContentStore? contentStore = _service.GetContentStore();
        ContentStore? mediaStore = _service.GetMediaStore();

        var contentStoreGen = contentStore?.GenCount;
        var mediaStoreGen = mediaStore?.GenCount;
        var contentStoreSnap = contentStore?.SnapCount;
        var mediaStoreSnap = mediaStore?.SnapCount;
        var contentStoreCount = contentStore?.Count;
        var mediaStoreCount = mediaStore?.Count;

        var contentStoreCountPlural = contentStoreCount > 1 ? "s" : string.Empty;
        var contentStoreGenPlural = contentStoreGen > 1 ? "s" : string.Empty;
        var contentStoreSnapPlural = contentStoreSnap > 1 ? "s" : string.Empty;
        var mediaStoreCountPlural = mediaStoreCount > 1 ? "s" : string.Empty;
        var mediaStoreGenPlural = mediaStoreGen > 1 ? "s" : string.Empty;
        var mediaStoreSnapPlural = mediaStoreSnap > 1 ? "s" : string.Empty;

        return
            $" Database cache is {dbCacheIsOk}. ContentStore contains {contentStoreCount} item{contentStoreCountPlural} and has {contentStoreGen} generation{contentStoreGenPlural} and {contentStoreSnap} snapshot{contentStoreSnapPlural}. MediaStore contains {mediaStoreCount} item{mediaStoreCountPlural} and has {mediaStoreGen} generation{mediaStoreGenPlural} and {mediaStoreSnap} snapshot{mediaStoreSnapPlural}.";
    }
}
