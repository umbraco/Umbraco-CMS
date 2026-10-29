using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Invalidates the cached member types that were moved to a different folder, so that other servers do not keep
/// serving them with a stale path and level.
/// </summary>
public sealed class MemberTypeMovedDistributedCacheNotificationHandler
    : MovedDistributedCacheNotificationHandlerBase<IMemberType, MemberTypeMovedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeMovedDistributedCacheNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public MemberTypeMovedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<MoveEventInfo<IMemberType>> entities, IDictionary<string, object?> state)
        => _distributedCache.RefreshMovedEntityTypeCache(entities);
}
