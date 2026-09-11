using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Invalidates the cached content types that were moved to a different folder, so that other servers do not keep
/// serving them with a stale path and level.
/// </summary>
public sealed class ContentTypeMovedDistributedCacheNotificationHandler
    : MovedDistributedCacheNotificationHandlerBase<IContentType, ContentTypeMovedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeMovedDistributedCacheNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public ContentTypeMovedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<MoveEventInfo<IContentType>> entities, IDictionary<string, object?> state)
        => _distributedCache.RefreshMovedEntityTypeCache(entities);
}
