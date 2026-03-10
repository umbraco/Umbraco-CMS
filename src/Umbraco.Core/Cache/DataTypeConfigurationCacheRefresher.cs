using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Handles <see cref="DataTypeCacheRefresherNotification" /> to clear the data type configuration cache.
/// </summary>
/// <remarks>
///     This handler ensures that the <see cref="IDataTypeConfigurationCache" /> is cleared when
///     data types are modified, keeping cached configurations in sync with the database.
/// </remarks>
internal sealed class DataTypeConfigurationCacheRefresher : INotificationHandler<DataTypeCacheRefresherNotification>
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeConfigurationCacheRefresher" /> class.
    /// </summary>
    /// <param name="dataTypeConfigurationCache">The data type configuration cache.</param>
    public DataTypeConfigurationCacheRefresher(IDataTypeConfigurationCache dataTypeConfigurationCache)
        => _dataTypeConfigurationCache = dataTypeConfigurationCache;

    /// <inheritdoc />
    public void Handle(DataTypeCacheRefresherNotification notification)
        => _dataTypeConfigurationCache.ClearCache(((DataTypeCacheRefresher.JsonPayload[])notification.MessageObject).Select(x => x.Key));
}
