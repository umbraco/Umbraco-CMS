using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// This cache is a temporary measure to reduce the amount of computational power required to deserialize and initialize <see cref="IDataType" /> when fetched from the main cache/database,
/// because datatypes are fetched multiple times troughout a (backoffice content) request with a lot of content (or nested content) and each of these fetches initializes certain fields on the datatypes.
/// </summary>
internal sealed class DataTypeConfigurationCache : IDataTypeConfigurationCache
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IConfigurationEditorJsonSerializer _jsonSerializer;
    private readonly IMemoryCache _memoryCache;

    // TODO KJA: constructor breakage
    public DataTypeConfigurationCache(IDataTypeService dataTypeService, IConfigurationEditorJsonSerializer jsonSerializer, IMemoryCache memoryCache)
    {
        _dataTypeService = dataTypeService;
        _memoryCache = memoryCache;
        _jsonSerializer = jsonSerializer;
    }

    public T? GetConfigurationAs<T>(Guid key)
        where T : class
    {
        var cacheKey = GetCacheKey(key);
        if (_memoryCache.TryGetValue(cacheKey, out T? configuration) is false)
        {
            IDataType? dataType = _dataTypeService.GetDataType(key);

            // Only cache if data type was found (but still cache null configurations)
            // TODO KJA: test what happens if this cache is accessed using the wrong type - i.e. TagConfiguration instead of MediaPicker3Configuration - will a null value be cached? that'd be a problem.
            if (dataType is not null)
            {
                var json = _jsonSerializer.Serialize(dataType.ConfigurationData);
                configuration = _jsonSerializer.Deserialize<T>(json);

                _memoryCache.Set(cacheKey, configuration);
            }
        }

        return configuration;
    }

    public void ClearCache(IEnumerable<Guid> keys)
    {
        foreach (Guid key in keys)
        {
            _memoryCache.Remove(GetCacheKey(key));
        }
    }

    private static string GetCacheKey(Guid key) => $"DataTypeConfigurationCache_{key}";
}
