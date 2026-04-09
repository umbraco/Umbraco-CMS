using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class RelationFactory
{
    /// <summary>
    /// Constructs an <see cref="Umbraco.Cms.Core.Models.IRelation"/> entity from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.RelationDto"/> and relation type.
    /// This method initializes the entity's properties based on the DTO, sets timestamps to UTC, and manages change tracking during construction.
    /// </summary>
    /// <param name="dto">The data transfer object containing the relation's data.</param>
    /// <param name="relationType">The relation type to associate with the new relation entity.</param>
    /// <returns>
    /// An <see cref="Umbraco.Cms.Core.Models.IRelation"/> entity populated with data from the specified DTO and relation type.
    /// </returns>
    public static IRelation BuildEntity(RelationDto dto, IRelationType relationType)
    {
        var entity = new Relation(dto.ParentId, dto.ChildId, dto.ParentObjectType, dto.ChildObjectType, relationType);

        try
        {
            entity.DisableChangeTracking();

            entity.Comment = dto.Comment;
            entity.CreateDate = dto.Datetime.EnsureUtc();
            entity.Id = dto.Id;
            entity.UpdateDate = dto.Datetime.EnsureUtc();

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
    /// Builds a <see cref="RelationDto"/> from the given <see cref="IRelation"/> entity.
    /// </summary>
    /// <param name="entity">The relation entity to convert to a DTO.</param>
    /// <returns>A <see cref="RelationDto"/> representing the given relation entity.</returns>
    public static RelationDto BuildDto(IRelation entity)
    {
        var dto = new RelationDto
        {
            ChildId = entity.ChildId,
            Comment = string.IsNullOrEmpty(entity.Comment) ? string.Empty : entity.Comment,
            Datetime = entity.CreateDate,
            ParentId = entity.ParentId,
            RelationType = entity.RelationType.Id,
        };

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }

    /// <summary>
    /// Builds a <see cref="RelationDto"/> from the given <see cref="IRelation"/> entity.
    /// </summary>
    /// <param name="entity">The relation entity to convert to a DTO.</param>
    /// <returns>A <see cref="RelationDto"/> representing the given relation entity.</returns>
    public static RelationDto BuildDto(ReadOnlyRelation entity)
    {
        var dto = new RelationDto
        {
            ChildId = entity.ChildId,
            Comment = string.IsNullOrEmpty(entity.Comment) ? string.Empty : entity.Comment,
            Datetime = entity.CreateDate,
            ParentId = entity.ParentId,
            RelationType = entity.RelationTypeId,
        };

        if (entity.HasIdentity)
        {
            dto.Id = entity.Id;
        }

        return dto;
    }
}
