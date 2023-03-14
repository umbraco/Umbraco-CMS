using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class RelationFactory
{
    public static IRelation BuildEntity(RelationDto dto, IRelationType relationType)
    {
        var entity = new Relation(dto.ParentId, dto.ChildId, dto.ParentObjectType, dto.ChildObjectType, relationType);

        try
        {
            entity.DisableChangeTracking();

            entity.Comment = dto.Comment;
            entity.CreateDate = dto.Datetime;
            entity.Id = dto.Id;
            entity.UpdateDate = dto.Datetime;

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }
        finally
        {
            entity.EnableChangeTracking();
        }
    }

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
