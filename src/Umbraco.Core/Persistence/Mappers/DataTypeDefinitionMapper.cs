using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="DataTypeDefinition"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(DataTypeDefinition))]
    [MapperFor(typeof(IDataTypeDefinition))]
    public sealed class DataTypeDefinitionMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<DataTypeDefinition, NodeDto>(src => src.Id, dto => dto.NodeId);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.CreateDate, dto => dto.CreateDate);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.Level, dto => dto.Level);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.Path, dto => dto.Path);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.SortOrder, dto => dto.SortOrder);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.Name, dto => dto.Text);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.Trashed, dto => dto.Trashed);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.Key, dto => dto.UniqueId);
            CacheMap<DataTypeDefinition, NodeDto>(src => src.CreatorId, dto => dto.UserId);
            CacheMap<DataTypeDefinition, DataTypeDto>(src => src.PropertyEditorAlias, dto => dto.PropertyEditorAlias);
            CacheMap<DataTypeDefinition, DataTypeDto>(src => src.DatabaseType, dto => dto.DbType);

        }
    }
}