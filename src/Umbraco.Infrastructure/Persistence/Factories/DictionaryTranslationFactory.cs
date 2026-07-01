using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DictionaryTranslationFactory
{
    #region Implementation of IEntityFactory<DictionaryTranslation,LanguageTextDto>

    /// <summary>
    /// Builds an <see cref="IDictionaryTranslation"/> entity from the given <see cref="LanguageTextDto"/>, unique identifier, and language.
    /// </summary>
    /// <param name="dto">The data transfer object containing language text data.</param>
    /// <param name="uniqueId">The unique identifier for the dictionary translation.</param>
    /// <param name="language">The language associated with the translation.</param>
    /// <returns>An <see cref="IDictionaryTranslation"/> entity constructed from the provided data.</returns>
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

    /// <summary>
    /// Creates a <see cref="LanguageTextDto"/> from the specified <see cref="IDictionaryTranslation"/> entity, assigning the provided unique identifier and resolving the language using the given ISO code mapping.
    /// </summary>
    /// <param name="entity">The dictionary translation entity to convert.</param>
    /// <param name="uniqueId">The unique identifier to assign to the DTO.</param>
    /// <param name="languagesByIsoCode">A dictionary mapping language ISO codes to <see cref="ILanguage"/> instances, used to resolve the language for the translation.</param>
    /// <returns>A <see cref="LanguageTextDto"/> representing the dictionary translation.</returns>
    /// <exception cref="ArgumentException">Thrown if the language specified by <paramref name="entity"/>'s ISO code is not found in <paramref name="languagesByIsoCode"/>.</exception>
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
