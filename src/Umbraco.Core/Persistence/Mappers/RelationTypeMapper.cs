using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

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

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<RelationType, RelationTypeDto>(src => src.Id, dto => dto.Id);
            CacheMap<RelationType, RelationTypeDto>(src => src.Alias, dto => dto.Alias);
            CacheMap<RelationType, RelationTypeDto>(src => src.ChildObjectType, dto => dto.ChildObjectType);
            CacheMap<RelationType, RelationTypeDto>(src => src.IsBidirectional, dto => dto.Dual);
            CacheMap<RelationType, RelationTypeDto>(src => src.Name, dto => dto.Name);
            CacheMap<RelationType, RelationTypeDto>(src => src.ParentObjectType, dto => dto.ParentObjectType);
        }
    }
}
