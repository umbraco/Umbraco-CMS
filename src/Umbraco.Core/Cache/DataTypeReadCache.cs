using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// This cache is a temporary measure to reduce the amount of computational power required to deserialize and initialize <see cref="IDataType"/> when fetched from the main cache/database,
/// because datatypes are fetched multiple times trough out a request with a lot of content (or nested content) and each of these fetches initializes certain fields on the datatypes.
/// </summary>
/// <remarks>This cache should never be used in an CUD path as it can hold stale values since the start of the request.</remarks>
public class DataTypeReadCache : IDataTypeReadCache
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IRequestCache _requestCache;

    // Does not need to be concurrent as it is all in one request and we (currently) do not run content processing logic in parallel.
    // Should never be null since this should be the only place where we set it trough the factory method.
    private Dictionary<int, IDataType?> DataTypes => (Dictionary<int, IDataType?>)_requestCache.Get("Umbraco_DataTypeReadCacheDictionary", () => new Dictionary<int, IDataType?>())!;

    public DataTypeReadCache(IDataTypeService dataTypeService, IRequestCache requestCache)
    {
        _dataTypeService = dataTypeService;
        _requestCache = requestCache;
    }


    /// <remarks>Do not use this to fetch a DataType that will be used in any non read part of any core services!</remarks>
    public IDataType? GetDataType(int id)
    {
        if (DataTypes.ContainsKey(id))
        {
            return DataTypes[id];
        }

        DataTypes[id] = _dataTypeService.GetDataType(id);
        return DataTypes[id];
    }
}
