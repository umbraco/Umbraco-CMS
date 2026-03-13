using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class TagFactory
{
    /// <summary>
    /// Creates an <see cref="Umbraco.Cms.Core.Models.ITag"/> entity from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.TagDto"/>.
    /// Initializes the tag's properties based on the data transfer object and resets its dirty property tracking.
    /// </summary>
    /// <param name="dto">The data transfer object containing the tag's data.</param>
    /// <returns>The constructed <see cref="Umbraco.Cms.Core.Models.ITag"/> entity.</returns>
    public static ITag BuildEntity(TagDto dto)
    {
        var entity = new Tag(dto.Id, dto.Group, dto.Text, dto.LanguageId) { NodeCount = dto.NodeCount };

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);
        return entity;
    }

    /// <summary>
    /// Creates a <see cref="TagDto"/> instance from the specified <see cref="ITag"/> entity.
    /// </summary>
    /// <param name="entity">The <see cref="ITag"/> entity to convert.</param>
    /// <returns>A <see cref="TagDto"/> that represents the provided tag entity.</returns>
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
