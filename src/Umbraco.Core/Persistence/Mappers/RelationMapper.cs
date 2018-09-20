using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

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

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache => PropertyInfoCacheInstance;

        protected override void BuildMap()
        {
            CacheMap<Relation, RelationDto>(src => src.Id, dto => dto.Id);
            CacheMap<Relation, RelationDto>(src => src.ChildId, dto => dto.ChildId);
            CacheMap<Relation, RelationDto>(src => src.Comment, dto => dto.Comment);
            CacheMap<Relation, RelationDto>(src => src.CreateDate, dto => dto.Datetime);
            CacheMap<Relation, RelationDto>(src => src.ParentId, dto => dto.ParentId);
            CacheMap<Relation, RelationDto>(src => src.RelationTypeId, dto => dto.RelationType);
        }
    }
}
