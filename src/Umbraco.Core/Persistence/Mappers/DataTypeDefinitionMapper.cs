using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="DataTypeDefinition"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    internal sealed class DataTypeDefinitionMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        internal static DataTypeDefinitionMapper Instance = new DataTypeDefinitionMapper();

        private DataTypeDefinitionMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
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
            CacheMap<DataTypeDefinition, NodeDto>(src => src.UserId, dto => dto.UserId);
            CacheMap<DataTypeDefinition, DataTypeDto>(src => src.ControlId, dto => dto.ControlId);
            CacheMap<DataTypeDefinition, DataTypeDto>(src => src.DatabaseType, dto => dto.DbType);

        }

        internal override string Map(string propertyName)
        {
            var dtoTypeProperty = PropertyInfoCache[propertyName];

            return base.GetColumnName(dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);
        }

        internal override void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = base.ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        #endregion
    }
}