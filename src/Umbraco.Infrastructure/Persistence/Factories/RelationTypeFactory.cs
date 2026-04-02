using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class RelationTypeFactory
{
    #region Implementation of IEntityFactory<RelationType,RelationTypeDto>

    /// <summary>
    /// Creates an <see cref="Umbraco.Cms.Core.Models.IRelationType"/> entity from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.RelationTypeDto"/>.
    /// </summary>
    /// <param name="dto">The data transfer object containing the relation type data to map from.</param>
    /// <returns>A new <see cref="Umbraco.Cms.Core.Models.IRelationType"/> entity populated with values from the <paramref name="dto"/>.</returns>
    public static IRelationType BuildEntity(RelationTypeDto dto)
    {
        var entity = new RelationType(dto.Name, dto.Alias, dto.Dual, dto.ParentObjectType, dto.ChildObjectType, dto.IsDependency);

        try
        {
            entity.DisableChangeTracking();

            entity.Id = dto.Id;
            entity.Key = dto.UniqueId;

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }
        finally
        {
            entity.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Creates a <see cref="RelationTypeDto"/> instance from the specified <see cref="IRelationType"/> entity.
    /// </summary>
    /// <param name="entity">The <see cref="IRelationType"/> entity to convert.</param>
    /// <returns>A <see cref="RelationTypeDto"/> that represents the provided relation type entity.</returns
    public static RelationTypeDto BuildDto(IRelationType entity)
    {
        var isDependency = false;
        if (entity is IRelationTypeWithIsDependency relationTypeWithIsDependency)
        {
            isDependency = relationTypeWithIsDependency.IsDependency;
        }

        var dto = new RelationTypeDto
        {
            Alias = entity.Alias,
            ChildObjectType = entity.ChildObjectType,
            Dual = entity.IsBidirectional,
            IsDependency = isDependency,
            Name = entity.Name ?? string.Empty,
            ParentObjectType = entity.ParentObjectType,
            UniqueId = entity.Key,
        };
        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }

    #endregion
}
