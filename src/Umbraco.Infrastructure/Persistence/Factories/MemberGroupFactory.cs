using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class MemberGroupFactory
{
    private static readonly Guid _nodeObjectTypeId;

    static MemberGroupFactory() => _nodeObjectTypeId = Constants.ObjectTypes.MemberGroup;

    #region Implementation of IEntityFactory<ITemplate,TemplateDto>

    public static IMemberGroup BuildEntity(NodeDto dto)
    {
        var group = new MemberGroup();

        try
        {
            group.DisableChangeTracking();

            group.CreateDate = dto.CreateDate;
            group.Id = dto.NodeId;
            group.Key = dto.UniqueId;
            group.Name = dto.Text;

            // reset dirty initial properties (U4-1946)
            group.ResetDirtyProperties(false);
            return group;
        }
        finally
        {
            group.EnableChangeTracking();
        }
    }

    public static NodeDto BuildDto(IMemberGroup entity)
    {
        var dto = new NodeDto
        {
            CreateDate = entity.CreateDate,
            NodeId = entity.Id,
            Level = 0,
            NodeObjectType = _nodeObjectTypeId,
            ParentId = -1,
            Path = string.Empty,
            SortOrder = 0,
            Text = entity.Name,
            Trashed = false,
            UniqueId = entity.Key,
            UserId = entity.CreatorId,
        };

        if (entity.HasIdentity)
        {
            dto.NodeId = entity.Id;
            dto.Path = "-1," + entity.Id;
        }

        return dto;
    }

    #endregion
}
