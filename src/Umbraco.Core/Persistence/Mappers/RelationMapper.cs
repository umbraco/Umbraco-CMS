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
    [MapperFor(typeof(IRelation))]
    [MapperFor(typeof(Relation))]
    public sealed class RelationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public RelationMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            CacheMap<Relation, RelationDto>(src => src.Id, dto => dto.Id);
            CacheMap<Relation, RelationDto>(src => src.ChildId, dto => dto.ChildId);
            CacheMap<Relation, RelationDto>(src => src.Comment, dto => dto.Comment);
            CacheMap<Relation, RelationDto>(src => src.CreateDate, dto => dto.Datetime);
            CacheMap<Relation, RelationDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<Relation, RelationDto>(src => src.RelationTypeId, dto => dto.RelationType);
        }

        #endregion
    }
}