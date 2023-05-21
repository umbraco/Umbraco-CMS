using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class TagFactory
{
    public static ITag BuildEntity(TagDto dto)
    {
        var entity = new Tag(dto.Id, dto.Group, dto.Text, dto.LanguageId) { NodeCount = dto.NodeCount };

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);
        return entity;
    }

    public static TagDto BuildDto(ITag entity) =>
        new TagDto
        {
            Id = entity.Id,
            Group = entity.Group,
            Text = entity.Text,
            LanguageId = entity.LanguageId,

            // Key = entity.Group + "/" + entity.Text // de-normalize
        };
}
