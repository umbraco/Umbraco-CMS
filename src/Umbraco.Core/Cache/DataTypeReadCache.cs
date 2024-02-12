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

    // does not need to be concurrent as it is all in one request and we (currently) do not run content processing logic in parallel.
    private readonly Dictionary<int, IDataType?> _dataTypes = new();

    public DataTypeReadCache(IDataTypeService dataTypeService)
    {
        _dataTypeService = dataTypeService;
    }


    /// <remarks>Do not use this to fetch a DataType that will be used in any non read part of any core services!</remarks>
    public IDataType? GetDataType(int id)
    {
        if (_dataTypes.ContainsKey(id))
        {
            return _dataTypes[id];
        }

        _dataTypes[id] = _dataTypeService.GetDataType(id);
        return _dataTypes[id];
    }
}
