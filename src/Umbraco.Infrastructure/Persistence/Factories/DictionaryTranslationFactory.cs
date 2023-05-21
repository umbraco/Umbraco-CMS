using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DictionaryTranslationFactory
{
    #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

    public static IDictionaryTranslation BuildEntity(LanguageTextDto dto, Guid uniqueId)
    {
        var item = new DictionaryTranslation(dto.LanguageId, dto.Value, uniqueId);

        try
        {
            item.DisableChangeTracking();

            item.Id = dto.PrimaryKey;

            // reset dirty initial properties (U4-1946)
            item.ResetDirtyProperties(false);
            return item;
        }
        finally
        {
            item.EnableChangeTracking();
        }
    }

    public static LanguageTextDto BuildDto(IDictionaryTranslation entity, Guid uniqueId)
    {
        var text = new LanguageTextDto { LanguageId = entity.LanguageId, UniqueId = uniqueId, Value = entity.Value };

        if (entity.HasIdentity)
        {
            text.PrimaryKey = entity.Id;
        }

        return text;
    }

    #endregion
}
