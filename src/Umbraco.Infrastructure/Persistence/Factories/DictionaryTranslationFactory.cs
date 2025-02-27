using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DictionaryTranslationFactory
{
    #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

    public static IDictionaryTranslation BuildEntity(LanguageTextDto dto, Guid uniqueId, ILanguage language)
    {
        var item = new DictionaryTranslation(language, dto.Value, uniqueId);

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

    public static LanguageTextDto BuildDto(IDictionaryTranslation entity, Guid uniqueId, IDictionary<string, ILanguage> languagesByIsoCode)
    {
        if (languagesByIsoCode.TryGetValue(entity.LanguageIsoCode, out ILanguage? language) == false)
        {
            throw new ArgumentException($"Could not find language with ISO code: {entity.LanguageIsoCode}", nameof(entity));
        }

        var text = new LanguageTextDto { LanguageId = language.Id, UniqueId = uniqueId, Value = entity.Value };

        if (entity.HasIdentity)
        {
            text.PrimaryKey = entity.Id;
        }

        return text;
    }

    #endregion
}
