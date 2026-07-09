using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IDataTypeConfigurationCache" /> to cache data type configurations.
/// </summary>
/// <remarks>
///     This cache is a temporary measure to reduce the amount of computational power required to
///     deserialize and initialize <see cref="IDataType" /> when fetched from the main cache/database,
///     because data types are fetched multiple times throughout a (backoffice content) request with
///     a lot of content (or nested content) and each of these fetches initializes certain fields on the data types.
/// </remarks>
internal sealed class DataTypeConfigurationCache : IDataTypeConfigurationCache
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeConfigurationCache" /> class.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="idKeyMap">The ID/key map service.</param>
    public DataTypeConfigurationCache(IDataTypeService dataTypeService, IMemoryCache memoryCache, IIdKeyMap idKeyMap)
    {
        _dataTypeService = dataTypeService;
        _memoryCache = memoryCache;
    }

    /// <inheritdoc />
    public T? GetConfigurationAs<T>(Guid key)
        where T : class
    {
        var cacheKey = GetCacheKey(key);
        if (_memoryCache.TryGetValue(cacheKey, out T? configuration) is false)
        {
            IDataType? dataType = _dataTypeService.GetAsync(key).GetAwaiter().GetResult();
            configuration = dataType?.ConfigurationAs<T>();

            // Only cache if data type was found (but still cache null configurations)
            if (dataType is not null)
            {
                _memoryCache.Set(cacheKey, configuration);
            }
        }

        return configuration;
    }

    /// <inheritdoc />
    public void ClearCache(IEnumerable<Guid> keys)
    {
        foreach (Guid key in keys)
        {
            _memoryCache.Remove(GetCacheKey(key));
        }
    }

    private static string GetCacheKey(Guid key) => $"DataTypeConfigurationCache_{key}";
}
