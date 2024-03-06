using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

internal sealed class DataTypeConfigurationCacheRefresher : INotificationHandler<DataTypeCacheRefresherNotification>
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    public DataTypeConfigurationCacheRefresher(IDataTypeConfigurationCache dataTypeConfigurationCache)
        => _dataTypeConfigurationCache = dataTypeConfigurationCache;

    public void Handle(DataTypeCacheRefresherNotification notification)
        => _dataTypeConfigurationCache.ClearCache(((DataTypeCacheRefresher.JsonPayload[])notification.MessageObject).Select(x => x.Key));
}
