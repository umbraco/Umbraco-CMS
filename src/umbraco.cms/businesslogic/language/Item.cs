using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Linq;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.businesslogic.language
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class is no longer used, nor should it ever be used, it will be removed from the codebase in future versions")]
    public class Item
    {

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Retrieves the value of a languagetranslated item given the key
        /// </summary>
        /// <param name="key">Unique identifier</param>
        /// <param name="languageId">Umbraco languageid</param>
        /// <returns>The language translated text</returns>
        public static string Text(Guid key, int languageId)
        {

            var item = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(key);
            if (item != null)
            {
                var translation = item.Translations.FirstOrDefault(x => x.Language.Id == languageId);
                if (translation != null)
                {
                    return translation.Value;
                }
            }

            throw new ArgumentException("Key being requested does not exist");
        }

        /// <summary>
        /// returns True if there is a value associated to the unique identifier with the specified language
        /// </summary>
        /// <param name="key">Unique identifier</param>
        /// <param name="languageId">Umbraco language id</param>
        /// <returns>returns True if there is a value associated to the unique identifier with the specified language</returns>
        public static bool hasText(Guid key, int languageId)
        {
            try
            {
                return Text(key, languageId).IsNullOrWhiteSpace() == false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Updates the value of the language translated item, throws an exeption if the
        /// key does not exist
        /// </summary>
        /// <param name="languageId">Umbraco language id</param>
        /// <param name="key">Unique identifier</param>
        /// <param name="value">The new dictionaryvalue</param>

        public static void setText(int languageId, Guid key, string value)
        {
            var lang = ApplicationContext.Current.Services.LocalizationService.GetLanguageById(languageId);
            if (lang == null) return;

            var item = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(key);
            if (item != null)
            {
                var translation = item.Translations.FirstOrDefault(x => x.Language.Id == languageId);
                if (translation == null)
                {
                    throw new ArgumentException("Key does not exist");
                }
                var newTranslations = new List<IDictionaryTranslation>(item.Translations)
                {
                    new DictionaryTranslation(lang, value)
                };
                item.Translations = newTranslations;
                ApplicationContext.Current.Services.LocalizationService.Save(item);
            }
        }

        /// <summary>
        /// Adds a new languagetranslated item to the collection
        /// 
        /// </summary>
        /// <param name="languageId">Umbraco languageid</param>
        /// <param name="key">Unique identifier</param>
        /// <param name="value"></param>
        public static void addText(int languageId, Guid key, string value)
        {
            var lang = ApplicationContext.Current.Services.LocalizationService.GetLanguageById(languageId);
            if (lang == null) return;

            var item = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(key);
            if (item != null)
            {
                var translation = item.Translations.FirstOrDefault(x => x.Language.Id == languageId);
                if (translation != null)
                {
                    throw new ArgumentException("Key being add'ed already exists");
                }
                var newTranslations = new List<IDictionaryTranslation>(item.Translations)
                {
                    new DictionaryTranslation(lang, value)
                };
                item.Translations = newTranslations;
                ApplicationContext.Current.Services.LocalizationService.Save(item);
            }
        }
        
        /// <summary>
        /// Removes all languagetranslated texts associated to the unique identifier.
        /// </summary>
        /// <param name="key">Unique identifier</param>
        public static void removeText(Guid key)
        {
            var found = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(key);
            if (found != null)
            {
                ApplicationContext.Current.Services.LocalizationService.Delete(found);    
            }
        }

        /// <summary>
        /// Removes all entries by language id.
        /// Primary used when deleting a language from Umbraco.
        /// </summary>
        /// <param name="languageId"></param>
        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        public static void RemoveByLanguage(int languageId)
        {
            var lang = ApplicationContext.Current.Services.LocalizationService.GetLanguageById(languageId);
            if (lang != null)
            {
                ApplicationContext.Current.Services.LocalizationService.Delete(lang);    
            }
        }
    }
}