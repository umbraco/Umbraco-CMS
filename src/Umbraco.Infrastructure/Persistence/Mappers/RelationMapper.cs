using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="Relation" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(IRelation))]
[MapperFor(typeof(Relation))]
public sealed class RelationMapper : BaseMapper
{
    public RelationMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<Relation, RelationDto>(nameof(Relation.Id), nameof(RelationDto.Id));
        DefineMap<Relation, RelationDto>(nameof(Relation.ChildId), nameof(RelationDto.ChildId));
        DefineMap<Relation, RelationDto>(nameof(Relation.Comment), nameof(RelationDto.Comment));
        DefineMap<Relation, RelationDto>(nameof(Relation.CreateDate), nameof(RelationDto.Datetime));
        DefineMap<Relation, RelationDto>(nameof(Relation.ParentId), nameof(RelationDto.ParentId));
        DefineMap<Relation, RelationDto>(nameof(Relation.RelationTypeId), nameof(RelationDto.RelationType));
    }
}
