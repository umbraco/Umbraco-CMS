using System;
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
        public DataTypeMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<DataType, NodeDto>(nameof(DataType.Id), nameof(NodeDto.NodeId));
            DefineMap<DataType, NodeDto>(nameof(DataType.CreateDate), nameof(NodeDto.CreateDate));
            DefineMap<DataType, NodeDto>(nameof(DataType.Level), nameof(NodeDto.Level));
            DefineMap<DataType, NodeDto>(nameof(DataType.ParentId), nameof(NodeDto.ParentId));
            DefineMap<DataType, NodeDto>(nameof(DataType.Path), nameof(NodeDto.Path));
            DefineMap<DataType, NodeDto>(nameof(DataType.SortOrder), nameof(NodeDto.SortOrder));
            DefineMap<DataType, NodeDto>(nameof(DataType.Name), nameof(NodeDto.Text));
            DefineMap<DataType, NodeDto>(nameof(DataType.Trashed), nameof(NodeDto.Trashed));
            DefineMap<DataType, NodeDto>(nameof(DataType.Key), nameof(NodeDto.UniqueId));
            DefineMap<DataType, NodeDto>(nameof(DataType.CreatorId), nameof(NodeDto.UserId));
            DefineMap<DataType, DataTypeDto>(nameof(DataType.EditorAlias), nameof(DataTypeDto.EditorAlias));
            DefineMap<DataType, DataTypeDto>(nameof(DataType.DatabaseType), nameof(DataTypeDto.DbType));
        }
    }
}
