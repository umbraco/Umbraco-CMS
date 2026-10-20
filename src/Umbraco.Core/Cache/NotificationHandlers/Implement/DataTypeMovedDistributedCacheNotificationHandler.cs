using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Invalidates the cached data types that were moved to a different folder, so that other servers do not keep
/// serving them with a stale path and level.
/// </summary>
public sealed class DataTypeMovedDistributedCacheNotificationHandler
    : MovedDistributedCacheNotificationHandlerBase<IDataType, DataTypeMovedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeMovedDistributedCacheNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public DataTypeMovedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<MoveEventInfo<IDataType>> entities, IDictionary<string, object?> state)
        => _distributedCache.RefreshMovedEntityTypeCache(entities);
}
