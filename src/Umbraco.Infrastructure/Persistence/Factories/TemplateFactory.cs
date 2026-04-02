using Umbraco.Cms.Core.Extensions;
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

    /// <summary>
    /// Constructs a <see cref="Umbraco.Cms.Core.Models.Template"/> entity from the provided data transfer object and related child definitions.
    /// </summary>
    /// <param name="shortStringHelper">The helper used for short string manipulation within the template.</param>
    /// <param name="dto">The <see cref="TemplateDto"/> containing the template's persisted data.</param>
    /// <param name="childDefinitions">A collection of child entities used to determine master template relationships.</param>
    /// <param name="getFileContent">A function that retrieves the content of a <see cref="Umbraco.Cms.Core.Models.File"/> associated with the template, or <c>null</c> if unavailable.</param>
    /// <returns>A fully constructed <see cref="Umbraco.Cms.Core.Models.Template"/> entity with properties populated from the DTO and related data.</returns>
    public static Template BuildEntity(
        IShortStringHelper shortStringHelper,
        TemplateDto dto,
        IEnumerable<IUmbracoEntity> childDefinitions,
        Func<File, string?> getFileContent)
    {
        var template = new Template(shortStringHelper, dto.NodeDto.Text, dto.Alias, getFileContent);

        try
        {
            template.DisableChangeTracking();

            template.CreateDate = dto.NodeDto.CreateDate.EnsureUtc();
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

    /// <summary>
    /// Creates a <see cref="TemplateDto"/> from the specified <see cref="Template"/> entity.
    /// </summary>
    /// <param name="entity">The template entity to convert.</param>
    /// <param name="nodeObjectTypeId">An optional node object type identifier.</param>
    /// <param name="primaryKey">The primary key value to assign to the resulting DTO.</param>
    /// <returns>A <see cref="TemplateDto"/> representing the provided template entity.</returns>
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
