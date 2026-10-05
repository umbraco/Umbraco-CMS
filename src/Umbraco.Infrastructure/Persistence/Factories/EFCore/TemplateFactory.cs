using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories.EFCore;

/// <summary>
///     EF Core factory for <see cref="ITemplate" />.
/// </summary>
internal static class TemplateFactory
{
    /// <summary>
    ///     Builds a <see cref="Template" /> from its persisted data.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="dto">The template DTO, with its <see cref="TemplateDto.NodeDto" /> loaded.</param>
    /// <param name="layoutTemplateAlias">The alias of the template's layout template, if it has one.</param>
    /// <param name="isLayoutTemplate">Whether other templates use this template as their layout.</param>
    /// <returns>The template entity.</returns>
    public static Template BuildEntity(
        IShortStringHelper shortStringHelper,
        TemplateDto dto,
        string? layoutTemplateAlias,
        bool isLayoutTemplate)
    {
        // The content isn't persisted with the template. Providing a loader leaves it unloaded, so that a caller
        // with access to the template's view file can supply one that reads it.
        var template = new Template(shortStringHelper, dto.NodeDto.Text, dto.Alias, _ => null);

        try
        {
            template.DisableChangeTracking();

            template.CreateDate = dto.NodeDto.CreateDate.EnsureUtc();
            template.Id = dto.NodeId;
            template.Key = dto.NodeDto.UniqueId;
            template.Path = dto.NodeDto.Path;
            template.IsLayoutTemplate = isLayoutTemplate;

            if (dto.NodeDto.ParentId > 0)
            {
                var layoutTemplateId = dto.NodeDto.ParentId;
                template.LayoutTemplateId = new Lazy<int>(() => layoutTemplateId);
                template.LayoutTemplateAlias = layoutTemplateAlias;
            }

            template.ResetDirtyProperties(false);
            return template;
        }
        finally
        {
            template.EnableChangeTracking();
        }
    }

    /// <summary>
    ///     Builds the <see cref="NodeDto" /> for a template.
    /// </summary>
    /// <param name="entity">The template.</param>
    /// <returns>The node DTO.</returns>
    public static NodeDto BuildNodeDto(ITemplate entity)
    {
        var layoutTemplateId = (entity as Template)?.LayoutTemplateId?.Value ?? Constants.System.Root;

        return new NodeDto
        {
            CreateDate = entity.CreateDate,
            NodeId = entity.Id,
            Level = 1,
            NodeObjectType = Constants.ObjectTypes.Template,
            ParentId = layoutTemplateId > 0 ? layoutTemplateId : Constants.System.Root,
            Path = entity.Path,
            Text = entity.Name,
            Trashed = false,
            UniqueId = entity.Key,
        };
    }
}
