using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a <see cref="RelationType"/> to DTO mapper used to translate the properties of the public api 
    /// implementation to that of the database's DTO as sql: [tableName].[columnName].
    /// </summary>
    [MapperFor(typeof(RelationType))]
    [MapperFor(typeof(IRelationType))]
    public sealed class RelationTypeMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public RelationTypeMapper()
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
            CacheMap<RelationType, RelationTypeDto>(src => src.Id, dto => dto.Id);
            CacheMap<RelationType, RelationTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<RelationType, RelationTypeDto>(src => src.ChildObjectType, dto => dto.ChildObjectType);
            CacheMap<RelationType, RelationTypeDto>(src => src.IsBidirectional, dto => dto.Dual);
            CacheMap<RelationType, RelationTypeDto>(src => src.Name, dto => dto.Name);
            CacheMap<RelationType, RelationTypeDto>(src => src.ParentObjectType, dto => dto.ParentObjectType);
        }

        #endregion
    }
}