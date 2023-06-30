using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class DataTypeDeletedDistributedCacheNotificationHandler : DeletedDistributedCacheNotificationHandlerBase<IDataType, DataTypeDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeDeletedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public DataTypeDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<IDataType> entities)
    {
        _distributedCache.RemoveDataTypeCache(entities);
        _distributedCache.RefreshValueEditorCache(entities);
    }
}
