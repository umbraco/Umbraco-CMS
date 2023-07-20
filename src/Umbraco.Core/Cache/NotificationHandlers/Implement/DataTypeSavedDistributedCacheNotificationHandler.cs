using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class DataTypeSavedDistributedCacheNotificationHandler : SavedDistributedCacheNotificationHandlerBase<IDataType, DataTypeSavedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeSavedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public DataTypeSavedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<IDataType> entities)
    {
        _distributedCache.RefreshDataTypeCache(entities);
        _distributedCache.RefreshValueEditorCache(entities);
    }
}
