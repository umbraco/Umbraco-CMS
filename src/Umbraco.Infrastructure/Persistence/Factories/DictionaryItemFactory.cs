using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DictionaryItemFactory
{
    /// <summary>
    /// Creates an <see cref="IDictionaryItem"/> entity from the specified <see cref="DictionaryDto"/>.
    /// </summary>
    /// <param name="dto">The DTO containing the data to populate the dictionary item entity.</param>
    /// <returns>An <see cref="IDictionaryItem"/> instance populated with data from the provided DTO.</returns>
    public static IDictionaryItem BuildEntity(DictionaryDto dto)
    {
        var item = new DictionaryItem(dto.Parent, dto.Key);

        try
        {
            item.DisableChangeTracking();

            item.Id = dto.PrimaryKey;
            item.Key = dto.UniqueId;

            item.ResetDirtyProperties(false);
            return item;
        }
        finally
        {
            item.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Builds a <see cref="DictionaryDto"/> from the given <see cref="IDictionaryItem"/> entity.
    /// </summary>
    /// <param name="entity">The dictionary item entity to convert.</param>
    /// <returns>A <see cref="DictionaryDto"/> representing the entity.</returns>
    public static DictionaryDto BuildDto(IDictionaryItem entity) =>
        new DictionaryDto
        {
            UniqueId = entity.Key,
            Key = entity.ItemKey,
            Parent = entity.ParentId,
            PrimaryKey = entity.Id
        };
}
