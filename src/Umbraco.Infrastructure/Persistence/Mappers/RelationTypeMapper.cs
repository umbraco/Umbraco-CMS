using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Represents a <see cref="RelationType" /> to DTO mapper used to translate the properties of the public api
///     implementation to that of the database's DTO as sql: [tableName].[columnName].
/// </summary>
[MapperFor(typeof(RelationType))]
[MapperFor(typeof(IRelationType))]
public sealed class RelationTypeMapper : BaseMapper
{
    public RelationTypeMapper(Lazy<ISqlContext> sqlContext, MapperConfigurationStore maps)
        : base(sqlContext, maps)
    {
    }

    protected override void DefineMaps()
    {
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.Id), nameof(RelationTypeDto.Id));
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.Alias), nameof(RelationTypeDto.Alias));
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.ChildObjectType), nameof(RelationTypeDto.ChildObjectType));
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.IsBidirectional), nameof(RelationTypeDto.Dual));
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.IsDependency), nameof(RelationTypeDto.IsDependency));
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.Name), nameof(RelationTypeDto.Name));
        DefineMap<RelationType, RelationTypeDto>(nameof(RelationType.ParentObjectType), nameof(RelationTypeDto.ParentObjectType));
    }
}
