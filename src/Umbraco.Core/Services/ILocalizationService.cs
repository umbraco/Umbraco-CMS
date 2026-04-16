using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the Localization Service, which is an easy access to operations involving Languages and Dictionary
/// </summary>
[Obsolete("Please use ILanguageService and IDictionaryItemService for localization. Scheduled for removal in Umbraco 18.")]
public interface ILocalizationService : IService
{
    // Possible to-do list:
    // Import DictionaryItem (?)
    // RemoveByLanguage (translations)
    // Add/Set Text (Insert/Update)
    // Remove Text (in translation)

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="int" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IDictionaryItem? GetDictionaryItemById(int id);

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="Guid" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IDictionaryItem? GetDictionaryItemById(Guid id);

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IDictionaryItem? GetDictionaryItemByKey(string key);

    /// <summary>
    ///     Gets a list of children for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId);

    /// <summary>
    ///     Saves a <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to save</param>
    /// <param name="userId">Optional id of the user saving the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    void Save(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a <see cref="ILanguage" /> by its id
    /// </summary>
    /// <param name="id">Id of the <see cref="ILanguage" /></param>
    /// <returns>
    ///     <see cref="ILanguage" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    ILanguage? GetLanguageById(int id);

    /// <summary>
    ///     Gets a <see cref="ILanguage" /> by its iso code
    /// </summary>
    /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
    /// <returns>
    ///     <see cref="ILanguage" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    ILanguage? GetLanguageByIsoCode(string? isoCode);

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    string GetDefaultLanguageIsoCode();

    /// <summary>
    ///     Gets all available languages
    /// </summary>
    /// <returns>An enumerable list of <see cref="ILanguage" /> objects</returns>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<ILanguage> GetAllLanguages();

    /// <summary>
    ///     Saves a <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to save</param>
    /// <param name="userId">Optional id of the user saving the language</param>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    void Save(ILanguage language, int userId = Constants.Security.SuperUserId);
}
