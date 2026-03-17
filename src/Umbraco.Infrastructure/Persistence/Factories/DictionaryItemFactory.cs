using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DictionaryItemFactory
{
    #region Implementation of IEntityFactory<DictionaryItem,DictionaryDto>

    /// <summary>
    /// Creates an <see cref="IDictionaryItem"/> entity from the specified <see cref="DictionaryDto"/>,
    /// mapping relevant properties and ensuring change tracking is properly managed during initialization.
    /// </summary>
    /// <param name="dto">The <see cref="DictionaryDto"/> containing the data to populate the dictionary item entity.</param>
    /// <returns>An <see cref="IDictionaryItem"/> instance populated with data from the provided DTO.</returns>
    public static IDictionaryItem BuildEntity(DictionaryDto dto)
    {
        var item = new DictionaryItem(dto.Parent, dto.Key);

        try
        {
            item.DisableChangeTracking();

            item.Id = dto.PrimaryKey;
            item.Key = dto.UniqueId;

            // reset dirty initial properties (U4-1946)
            item.ResetDirtyProperties(false);
            return item;
        }
        finally
        {
            item.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Builds a <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DictionaryDto"/> from the given <see cref="Umbraco.Cms.Core.Models.IDictionaryItem"/> entity.
    /// </summary>
    /// <param name="entity">The dictionary item entity to convert.</param>
    /// <returns>A <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DictionaryDto"/> representing the entity.</returns>
    public static DictionaryDto BuildDto(IDictionaryItem entity) =>
        new DictionaryDto
        {
            UniqueId = entity.Key,
            Key = entity.ItemKey,
            Parent = entity.ParentId,
            PrimaryKey = entity.Id
        };

    #endregion
}
