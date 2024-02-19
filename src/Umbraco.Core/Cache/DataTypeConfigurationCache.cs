using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// This cache is a temporary measure to reduce the amount of computational power required to deserialize and initialize <see cref="IDataType" /> when fetched from the main cache/database,
/// because datatypes are fetched multiple times troughout a (backoffice content) request with a lot of content (or nested content) and each of these fetches initializes certain fields on the datatypes.
/// </summary>
internal sealed class DataTypeConfigurationCache : IDataTypeConfigurationCache, INotificationHandler<DataTypeCacheRefresherNotification>
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentBag<string> _cacheKeys = new ConcurrentBag<string>();

    public DataTypeConfigurationCache(IDataTypeService dataTypeService, IMemoryCache memoryCache)
    {
        _dataTypeService = dataTypeService;
        _memoryCache = memoryCache;
    }

    public T? GetConfigurationAs<T>(int id)
        where T : class
    {
        var cacheKey = GetCacheKey(id);
        if (_memoryCache.TryGetValue(cacheKey, out T? configuration))
        {
            return configuration;
        }

        return GetConfiguration<T>(_dataTypeService.GetDataType(id));
    }

    private T? GetConfiguration<T>(IDataType? dataType)
        where T : class
    {
        T? configuration = dataType?.ConfigurationAs<T>();

        if (dataType is not null)
        {
            var cacheKey = GetCacheKey(dataType.Id);
            _memoryCache.Set(cacheKey, configuration);
            _cacheKeys.Add(cacheKey);
        }

        return configuration;
    }

    public void Handle(DataTypeCacheRefresherNotification notification) => ClearCache();

    private static string GetCacheKey(int id) => $"DataTypeConfigurationCache_{id}";

    private void ClearCache()
    {
        foreach (var key in _cacheKeys)
        {
            _memoryCache.Remove(key);
        }

        _cacheKeys.Clear();
    }
}
