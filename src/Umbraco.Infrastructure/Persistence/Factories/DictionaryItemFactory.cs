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

    private static List<LanguageTextDto> BuildLanguageTextDtos(IDictionaryItem entity)
    {
        var list = new List<LanguageTextDto>();
        if (entity.Translations is not null)
        {
            foreach (IDictionaryTranslation translation in entity.Translations)
            {
                var text = new LanguageTextDto
                {
                    LanguageId = translation.LanguageId,
                    UniqueId = translation.Key,
                    Value = translation.Value,
                };

                if (translation.HasIdentity)
                {
                    text.PrimaryKey = translation.Id;
                }

                list.Add(text);
            }
        }

        return list;
    }

    public static DictionaryDto BuildDto(IDictionaryItem entity) =>
        new DictionaryDto
        {
            UniqueId = entity.Key,
            Key = entity.ItemKey,
            Parent = entity.ParentId,
            PrimaryKey = entity.Id,
            LanguageTextDtos = BuildLanguageTextDtos(entity),
        };

    #endregion
}
