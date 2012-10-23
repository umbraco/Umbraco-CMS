using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="Relation"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    internal sealed class RelationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        internal static RelationMapper Instance = new RelationMapper();

        private RelationMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            CacheMap<Relation, RelationDto>(src => src.Id, dto => dto.Id);
            CacheMap<Relation, RelationDto>(src => src.ChildId, dto => dto.ChildId);
            CacheMap<Relation, RelationDto>(src => src.Comment, dto => dto.Comment);
            CacheMap<Relation, RelationDto>(src => src.CreateDate, dto => dto.Datetime);
            CacheMap<Relation, RelationDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<Relation, RelationDto>(src => src.RelationType.Id, dto => dto.RelationType);
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