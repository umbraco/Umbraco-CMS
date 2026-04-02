using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class MemberGroupFactory
{
    private static readonly Guid _nodeObjectTypeId;

    static MemberGroupFactory() => _nodeObjectTypeId = Constants.ObjectTypes.MemberGroup;

    #region Implementation of IEntityFactory<ITemplate,TemplateDto>

    /// <summary>
    /// Builds an <see cref="Umbraco.Cms.Core.Models.IMemberGroup"/> entity from the given <see cref="NodeDto"/>.
    /// </summary>
    /// <param name="dto">The data transfer object containing the member group data.</param>
    /// <returns>An <see cref="Umbraco.Cms.Core.Models.IMemberGroup"/> instance populated with data from the DTO.</returns>
    public static IMemberGroup BuildEntity(NodeDto dto)
    {
        var group = new MemberGroup();

        try
        {
            group.DisableChangeTracking();

            group.CreateDate = dto.CreateDate.EnsureUtc();
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

    /// <summary>
    /// Creates and returns a <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.NodeDto"/> that represents the specified <see cref="Umbraco.Cms.Core.Models.IMemberGroup"/> entity.
    /// </summary>
    /// <param name="entity">The member group entity from which to construct the <see cref="NodeDto"/>.</param>
    /// <returns>
    /// A <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.NodeDto"/> populated with values from the given member group, including its ID, name, creation date, creator ID, and unique key.
    /// </returns>
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
