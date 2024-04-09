using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DictionaryItemFactory
{
    #region Implementation of IEntityFactory<DictionaryItem,DictionaryDto>

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
