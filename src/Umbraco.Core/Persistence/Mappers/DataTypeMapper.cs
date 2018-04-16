using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="DataType"/> to DTO mapper used to translate the properties of the public api
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(DataType))]
    [MapperFor(typeof(IDataType))]
    public sealed class DataTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<DataType, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<DataType, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<DataType, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<DataType, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<DataType, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<DataType, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<DataType, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<DataType, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<DataType, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<DataType, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<DataType, DataTypeDto>(src => src.EditorAlias, dto => dto.EditorAlias);
            CacheMap<DataType, DataTypeDto>(src => src.DatabaseType, dto => dto.DbType);

        }
    }
}
