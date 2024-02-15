using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// This cache is a temporary measure to reduce the amount of computational power required to deserialize and initialize <see cref="IDataType"/> when fetched from the main cache/database,
/// because datatypes are fetched multiple times trough out a (backoffice content) request with a lot of content (or nested content) and each of these fetches initializes certain fields on the datatypes.
/// </summary>
/// <remarks>This cache should never be used in an CUD path as it can hold stale values.</remarks>
public sealed class DataTypeReadCache : IDataTypeReadCache, INotificationHandler<DataTypeSavedNotification>,
    INotificationHandler<DataTypeDeletedNotification>
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentBag<string> _cacheKeys = new ConcurrentBag<string>();

    public DataTypeReadCache(IDataTypeService dataTypeService, IMemoryCache memoryCache)
    {
        _dataTypeService = dataTypeService;
        _memoryCache = memoryCache;
    }

    /// <remarks>Do not use this to fetch a DataType that will be used in any non read part of any core services!</remarks>
    public IDataType? GetDataType(int id)
    {
        var key = GetCacheKey(id);
        if (_memoryCache.TryGetValue(key, out IDataType? dataType))
        {
            return dataType;
        }

        dataType = _dataTypeService.GetDataType(id);
        if (dataType is null)
        {
            return dataType;
        }

        _cacheKeys.Add(key);
        _memoryCache.Set(key, dataType);

        return dataType;
    }

    private string GetCacheKey(int id) => $"DataTypeReadCache_{id}";

    private void ClearCache()
    {
        foreach (var key in _cacheKeys)
        {
            _memoryCache.Remove(key);
        }

        _cacheKeys.Clear();
    }

    public void Handle(DataTypeSavedNotification notification) => ClearCache();

    public void Handle(DataTypeDeletedNotification notification) => ClearCache();
}
