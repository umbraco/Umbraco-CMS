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
    ///     Creates and saves a new dictionary item and assigns a value to all languages if defaultValue is specified.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="parentId"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IDictionaryItem CreateDictionaryItemWithIdentity(string key, Guid? parentId, string? defaultValue = null);

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
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their <see cref="Guid" /> ids
    /// </summary>
    /// <param name="ids">Ids of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemsByIds(params Guid[] ids) => Array.Empty<IDictionaryItem>();

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
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their keys
    /// </summary>
    /// <param name="keys">Keys of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemsByKeys(params string[] keys) => Array.Empty<IDictionaryItem>();

    /// <summary>
    ///     Gets a list of children for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId);

    /// <summary>
    ///     Gets a list of descendants for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent, null will return all dictionary items</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId);

    /// <summary>
    ///     Gets the root/top <see cref="IDictionaryItem" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IDictionaryItem> GetRootDictionaryItems();

    /// <summary>
    ///     Checks if a <see cref="IDictionaryItem" /> with given key exists
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>True if a <see cref="IDictionaryItem" /> exists, otherwise false</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    bool DictionaryItemExists(string key);

    /// <summary>
    ///     Saves a <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to save</param>
    /// <param name="userId">Optional id of the user saving the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    void Save(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a <see cref="IDictionaryItem" /> object and its related translations
    ///     as well as its children.
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Scheduled for removal in Umbraco 18.")]
    void Delete(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId);

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
    ///     Gets a language identifier from its ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    int? GetLanguageIdByIsoCode(string isoCode);

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    string GetDefaultLanguageIsoCode();

    /// <summary>
    ///     Gets the default language identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    Guid? GetDefaultLanguageKey();

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

    /// <summary>
    ///     Deletes a <see cref="ILanguage" /> by removing it and its usages from the db
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the language</param>
    [Obsolete("Please use ILanguageService for language operations. Scheduled for removal in Umbraco 18.")]
    void Delete(ILanguage language, int userId = Constants.Security.SuperUserId);
}
