using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using File = Umbraco.Cms.Core.Models.File;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class TemplateFactory
{
    private static NodeDto BuildNodeDto(Template entity, Guid? nodeObjectTypeId)
    {
        var nodeDto = new NodeDto
        {
            CreateDate = entity.CreateDate,
            NodeId = entity.Id,
            Level = 1,
            NodeObjectType = nodeObjectTypeId,
            ParentId = entity.MasterTemplateId?.Value ?? 0,
            Path = entity.Path,
            Text = entity.Name,
            Trashed = false,
            UniqueId = entity.Key
        };

        return nodeDto;
    }

    #region Implementation of IEntityFactory<ITemplate,TemplateDto>

    public static Template BuildEntity(IShortStringHelper shortStringHelper, TemplateDto dto,
        IEnumerable<IUmbracoEntity> childDefinitions, Func<File, string?> getFileContent)
    {
        var template = new Template(shortStringHelper, dto.NodeDto.Text, dto.Alias, getFileContent);

        try
        {
            template.DisableChangeTracking();

            template.CreateDate = dto.NodeDto.CreateDate;
            template.Id = dto.NodeId;
            template.Key = dto.NodeDto.UniqueId;
            template.Path = dto.NodeDto.Path;

            template.IsMasterTemplate = childDefinitions.Any(x => x.ParentId == dto.NodeId);

            if (dto.NodeDto.ParentId > 0)
            {
                template.MasterTemplateId = new Lazy<int>(() => dto.NodeDto.ParentId);
            }

            // reset dirty initial properties (U4-1946)
            template.ResetDirtyProperties(false);
            return template;
        }
        finally
        {
            template.EnableChangeTracking();
        }
    }

    public static TemplateDto BuildDto(Template entity, Guid? nodeObjectTypeId, int primaryKey)
    {
        var dto = new TemplateDto {Alias = entity.Alias, NodeDto = BuildNodeDto(entity, nodeObjectTypeId)};

        if (entity.MasterTemplateId != null && entity.MasterTemplateId.Value > 0)
        {
            dto.NodeDto.ParentId = entity.MasterTemplateId.Value;
        }

        if (entity.HasIdentity)
        {
            dto.NodeId = entity.Id;
            dto.PrimaryKey = primaryKey;
        }

        return dto;
    }

    #endregion
}
