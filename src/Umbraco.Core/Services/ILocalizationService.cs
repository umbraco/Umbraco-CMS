using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the Localization Service, which is an easy access to operations involving Languages and Dictionary
/// </summary>
[Obsolete("Please use ILanguageService and IDictionaryItemService for localization. Will be removed in V15.")]
public interface ILocalizationService : IService
{
    // Possible to-do list:
    // Import DictionaryItem (?)
    // RemoveByLanguage (translations)
    // Add/Set Text (Insert/Update)
    // Remove Text (in translation)

    /// <summary>
    ///     Adds or updates a translation for a dictionary item and language
    /// </summary>
    /// <param name="item"></param>
    /// <param name="language"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    void AddOrUpdateDictionaryValue(IDictionaryItem item, ILanguage? language, string value);

    /// <summary>
    ///     Creates and saves a new dictionary item and assigns a value to all languages if defaultValue is specified.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="parentId"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IDictionaryItem CreateDictionaryItemWithIdentity(string key, Guid? parentId, string? defaultValue = null);

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="int" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IDictionaryItem? GetDictionaryItemById(int id);

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its <see cref="Guid" /> id
    /// </summary>
    /// <param name="id">Id of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IDictionaryItem? GetDictionaryItemById(Guid id);

    /// <summary>
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their <see cref="Guid" /> ids
    /// </summary>
    /// <param name="ids">Ids of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemsByIds(params Guid[] ids) => Array.Empty<IDictionaryItem>();

    /// <summary>
    ///     Gets a <see cref="IDictionaryItem" /> by its key
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IDictionaryItem? GetDictionaryItemByKey(string key);

    /// <summary>
    ///     Gets a collection of <see cref="IDictionaryItem" /> by their keys
    /// </summary>
    /// <param name="keys">Keys of the <see cref="IDictionaryItem" /></param>
    /// <returns>
    ///     A collection of <see cref="IDictionaryItem" />
    /// </returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemsByKeys(params string[] keys) => Array.Empty<IDictionaryItem>();

    /// <summary>
    ///     Gets a list of children for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId);

    /// <summary>
    ///     Gets a list of descendants for a <see cref="IDictionaryItem" />
    /// </summary>
    /// <param name="parentId">Id of the parent, null will return all dictionary items</param>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IEnumerable<IDictionaryItem> GetDictionaryItemDescendants(Guid? parentId);

    /// <summary>
    ///     Gets the root/top <see cref="IDictionaryItem" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="IDictionaryItem" /> objects</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    IEnumerable<IDictionaryItem> GetRootDictionaryItems();

    /// <summary>
    ///     Checks if a <see cref="IDictionaryItem" /> with given key exists
    /// </summary>
    /// <param name="key">Key of the <see cref="IDictionaryItem" /></param>
    /// <returns>True if a <see cref="IDictionaryItem" /> exists, otherwise false</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    bool DictionaryItemExists(string key);

    /// <summary>
    ///     Saves a <see cref="IDictionaryItem" /> object
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to save</param>
    /// <param name="userId">Optional id of the user saving the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    void Save(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a <see cref="IDictionaryItem" /> object and its related translations
    ///     as well as its children.
    /// </summary>
    /// <param name="dictionaryItem"><see cref="IDictionaryItem" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the dictionary item</param>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    void Delete(IDictionaryItem dictionaryItem, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets a <see cref="ILanguage" /> by its id
    /// </summary>
    /// <param name="id">Id of the <see cref="ILanguage" /></param>
    /// <returns>
    ///     <see cref="ILanguage" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    ILanguage? GetLanguageById(int id);

    /// <summary>
    ///     Gets a <see cref="ILanguage" /> by its iso code
    /// </summary>
    /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
    /// <returns>
    ///     <see cref="ILanguage" />
    /// </returns>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    ILanguage? GetLanguageByIsoCode(string? isoCode);

    /// <summary>
    ///     Gets a language identifier from its ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    int? GetLanguageIdByIsoCode(string isoCode);

    /// <summary>
    ///     Gets a language ISO code from its identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    string? GetLanguageIsoCodeById(int id);

    /// <summary>
    ///     Gets the default language ISO code.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    string GetDefaultLanguageIsoCode();

    /// <summary>
    ///     Gets the default language identifier.
    /// </summary>
    /// <remarks>
    ///     <para>This can be optimized and bypass all deep cloning.</para>
    /// </remarks>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    int? GetDefaultLanguageId();

    /// <summary>
    ///     Gets all available languages
    /// </summary>
    /// <returns>An enumerable list of <see cref="ILanguage" /> objects</returns>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    IEnumerable<ILanguage> GetAllLanguages();

    /// <summary>
    ///     Saves a <see cref="ILanguage" /> object
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to save</param>
    /// <param name="userId">Optional id of the user saving the language</param>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    void Save(ILanguage language, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Deletes a <see cref="ILanguage" /> by removing it and its usages from the db
    /// </summary>
    /// <param name="language"><see cref="ILanguage" /> to delete</param>
    /// <param name="userId">Optional id of the user deleting the language</param>
    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    void Delete(ILanguage language, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets the full dictionary key map.
    /// </summary>
    /// <returns>The full dictionary key map.</returns>
    [Obsolete("Please use IDictionaryItemService for dictionary item operations. Will be removed in V15.")]
    Dictionary<string, Guid> GetDictionaryItemKeyMap();

    [Obsolete("Please use ILanguageService for language operations. Will be removed in V15.")]
    PagedModel<ILanguage> GetAllLanguagesPaged(int skip, int take)
    {
        ILanguage[] all = GetAllLanguages().Skip(skip).Take(take).ToArray();
        return new PagedModel<ILanguage>(all.Length, all);
    }
}
