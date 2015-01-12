using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the Localization Service, which is an easy access to operations involving Languages and Dictionary
    /// </summary>
    public interface ILocalizationService : IService
    {
        //Possible to-do list:
        //Import DictionaryItem (?)
        //RemoveByLanguage (translations)
        //Add/Set Text (Insert/Update)
        //Remove Text (in translation)

        /// <summary>
        /// Adds or updates a translation for a dictionary item and language
        /// </summary>
        /// <param name="item"></param>
        /// <param name="language"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void AddOrUpdateDictionaryValue(IDictionaryItem item, ILanguage language, string value);

        /// <summary>
        /// Creates and saves a new dictionary item and assigns a value to all languages if defaultValue is specified.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parentId"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        IDictionaryItem CreateDictionaryItemWithIdentity(string key, Guid? parentId, string defaultValue = null);

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Int32"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        IDictionaryItem GetDictionaryItemById(int id);

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Guid"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        IDictionaryItem GetDictionaryItemById(Guid id);

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its key
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        IDictionaryItem GetDictionaryItemByKey(string key);

        /// <summary>
        /// Gets a list of children for a <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId);

        /// <summary>
        /// Gets the root/top <see cref="IDictionaryItem"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        IEnumerable<IDictionaryItem> GetRootDictionaryItems();

        /// <summary>
        /// Checks if a <see cref="IDictionaryItem"/> with given key exists
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns>True if a <see cref="IDictionaryItem"/> exists, otherwise false</returns>
        bool DictionaryItemExists(string key);

        /// <summary>
        /// Saves a <see cref="IDictionaryItem"/> object
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to save</param>
        /// <param name="userId">Optional id of the user saving the dictionary item</param>
        void Save(IDictionaryItem dictionaryItem, int userId = 0);

        /// <summary>
        /// Deletes a <see cref="IDictionaryItem"/> object and its related translations
        /// as well as its children.
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the dictionary item</param>
        void Delete(IDictionaryItem dictionaryItem, int userId = 0);

        /// <summary>
        /// Gets a <see cref="ILanguage"/> by its id
        /// </summary>
        /// <param name="id">Id of the <see cref="ILanguage"/></param>
        /// <returns><see cref="ILanguage"/></returns>
        ILanguage GetLanguageById(int id);

        /// <summary>
        /// Gets a <see cref="ILanguage"/> by its culture code
        /// </summary>
        /// <param name="cultureName">Culture Code - also refered to as the Friendly name</param>
        /// <returns><see cref="ILanguage"/></returns>
        ILanguage GetLanguageByCultureCode(string cultureName);

        /// <summary>
        /// Gets a <see cref="Language"/> by its iso code
        /// </summary>
        /// <param name="isoCode">Iso Code of the language (ie. en-US)</param>
        /// <returns><see cref="Language"/></returns>
        ILanguage GetLanguageByIsoCode(string isoCode);

        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>An enumerable list of <see cref="ILanguage"/> objects</returns>
        IEnumerable<ILanguage> GetAllLanguages();

        /// <summary>
        /// Saves a <see cref="ILanguage"/> object
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to save</param>
        /// <param name="userId">Optional id of the user saving the language</param>
        void Save(ILanguage language, int userId = 0);

        /// <summary>
        /// Deletes a <see cref="ILanguage"/> by removing it and its usages from the db
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the language</param>
        void Delete(ILanguage language, int userId = 0);
    }
}