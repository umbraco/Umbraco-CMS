using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

/// <summary>
///     Queues deferred cache rebuilds in response to content type and media type structural changes.
/// </summary>
/// <remarks>
///     Handles <see cref="ContentTypeChangedNotification" /> and <see cref="MediaTypeChangedNotification" />,
///     which fire <b>after</b> the database transaction is committed. This avoids lock contention between the
///     deferred rebuild's background transaction and the original save transaction.
/// </remarks>
internal sealed class DeferredCacheRebuildNotificationHandler :
    INotificationHandler<ContentTypeChangedNotification>,
    INotificationHandler<MediaTypeChangedNotification>
{
    private readonly IDeferredCacheRebuildService _deferredCacheRebuildService;
    private readonly CacheSettings _cacheSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeferredCacheRebuildNotificationHandler" /> class.
    /// </summary>
    /// <param name="deferredCacheRebuildService">The deferred cache rebuild service.</param>
    /// <param name="cacheSettings">The cache settings.</param>
    public DeferredCacheRebuildNotificationHandler(
        IDeferredCacheRebuildService deferredCacheRebuildService,
        IOptions<CacheSettings> cacheSettings)
    {
        _deferredCacheRebuildService = deferredCacheRebuildService;
        _cacheSettings = cacheSettings.Value;
    }

    /// <inheritdoc />
    public void Handle(ContentTypeChangedNotification notification)
    {
        if (_cacheSettings.ContentTypeRebuildMode != ContentTypeRebuildMode.Deferred)
        {
            return;
        }

        var structuralChangeIds = notification.Changes
            .Where(x => x.ChangeTypes.IsStructuralChange())
            .Select(x => x.Item.Id)
            .ToArray();

        if (structuralChangeIds.Length > 0)
        {
            _deferredCacheRebuildService.QueueContentTypeRebuild(structuralChangeIds);
        }
    }

    /// <inheritdoc />
    public void Handle(MediaTypeChangedNotification notification)
    {
        if (_cacheSettings.ContentTypeRebuildMode != ContentTypeRebuildMode.Deferred)
        {
            return;
        }

        var structuralChangeIds = notification.Changes
            .Where(x => x.ChangeTypes.IsStructuralChange())
            .Select(x => x.Item.Id)
            .ToArray();

        if (structuralChangeIds.Length > 0)
        {
            _deferredCacheRebuildService.QueueMediaTypeRebuild(structuralChangeIds);
        }
    }
}
