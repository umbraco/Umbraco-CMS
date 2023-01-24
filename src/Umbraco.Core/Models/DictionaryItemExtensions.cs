using Umbraco.Cms.Core.Models;

namespace Umbraco.Extensions;

public static class DictionaryItemExtensions
{
    /// <summary>
    ///     Returns the translation value for the language id, if no translation is found it returns an empty string
    /// </summary>
    /// <param name="d"></param>
    /// <param name="languageId"></param>
    /// <returns></returns>
    public static string? GetTranslatedValue(this IDictionaryItem d, int languageId)
    {
        IDictionaryTranslation? trans = d.Translations.FirstOrDefault(x => x.LanguageId == languageId);
        return trans == null ? string.Empty : trans.Value;
    }

    /// <summary>
    ///     Returns the default translated value based on the default language
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static string? GetDefaultValue(this IDictionaryItem d)
    {
        IDictionaryTranslation? defaultTranslation = d.Translations.FirstOrDefault(x => x.Language?.Id == 1);
        return defaultTranslation == null ? string.Empty : defaultTranslation.Value;
    }

    /// <summary>
    ///     Adds or updates a translation for a dictionary item and language
    /// </summary>
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <param name="value"></param>
    public static void AddOrUpdateDictionaryValue(this IDictionaryItem item, ILanguage language, string value)
    {
        IDictionaryTranslation? existing = item.Translations?.FirstOrDefault(x => x.Language?.Id == language.Id);
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            if (item.Translations is not null)
            {
                item.Translations = new List<IDictionaryTranslation>(item.Translations)
                {
                    new DictionaryTranslation(language, value),
                };
            }
            else
            {
                item.Translations = new List<IDictionaryTranslation> { new DictionaryTranslation(language, value) };
            }
        }
    }
}
