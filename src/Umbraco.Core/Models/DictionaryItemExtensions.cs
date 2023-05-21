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
    [Obsolete("This will be replaced in V13 by a corresponding method accepting language ISO code instead of language ID.")]
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
    [Obsolete("Warning: This method ONLY works in very specific scenarios. It will be removed in V13.")]
    public static string? GetDefaultValue(this IDictionaryItem d)
    {
        IDictionaryTranslation? defaultTranslation = d.Translations.FirstOrDefault(x => x.Language?.Id == 1);
        return defaultTranslation == null ? string.Empty : defaultTranslation.Value;
    }
}
