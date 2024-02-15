using System.Collections.Concurrent;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// This cache is a temporary measure to reduce the amount of computational power required to deserialize and initialize <see cref="IDataType"/> when fetched from the main cache/database,
/// because datatypes are fetched multiple times trough out a request with a lot of content (or nested content) and each of these fetches initializes certain fields on the datatypes.
/// </summary>
/// <remarks>This cache should never be used in an CUD path as it can hold stale values.</remarks>
public sealed class DataTypeReadCache : IDataTypeReadCache, INotificationHandler<DataTypeSavedNotification>, INotificationHandler<DataTypeDeletedNotification>
{
    private readonly IDataTypeService _dataTypeService;

    private ConcurrentDictionary<int, IDataType?> DataTypes => new ConcurrentDictionary<int, IDataType?>();

    public DataTypeReadCache(IDataTypeService dataTypeService, IRequestCache requestCache)
    {
        _dataTypeService = dataTypeService;
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

    private void ClearCache() => DataTypes.Clear();

    public void Handle(DataTypeSavedNotification notification) => ClearCache();

    public void Handle(DataTypeDeletedNotification notification) => ClearCache();
}
