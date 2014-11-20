using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;

using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.businesslogic.language
{
    /// <summary>
    /// THIS CLASS IS NOT INTENDED TO BE USED DIRECTLY IN YOUR CODE, USE THE umbraco.cms.businesslogic.Dictionary class instead
    /// </summary>
    /// <remarks>
    /// This class is used by the DictionaryItem, all caching is handled in the DictionaryItem.Save() method which will ensure that
    /// cache is invalidated if anything is changed.
    /// </remarks>
    [Obsolete("THIS CLASS IS NOT INTENDED TO BE USED DIRECTLY IN YOUR CODE, USE THE umbraco.cms.businesslogic.Dictionary class instead")]
    public class Item
    {
        private static readonly ConcurrentDictionary<Guid, Dictionary<int, string>> Items = new ConcurrentDictionary<Guid, Dictionary<int, string>>();        
        private static volatile bool _isInitialize;
        private static readonly object Locker = new object();

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
        /// Populates the global hash table with the data from the database.
        /// </summary>
        private static void EnsureCache()
        {
            if (!_isInitialize)
            {
                lock (Locker)
                {
                    //double check
                    if (!_isInitialize)
                    {

                        var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<LanguageTextDto>("ORDER BY UniqueId");
                        foreach (var dto in dtos)
                        {
                            var languageId = dto.LanguageId;
                            var uniqueId = dto.UniqueId;
                            var text = dto.Value;

                            Items.AddOrUpdate(uniqueId, guid =>
                            {
                                var languagevalues = new Dictionary<int, string> { { languageId, text } };
                                return languagevalues;
                            }, (guid, dictionary) =>
                            {
                                // add/update the text for the id
                                dictionary[languageId] = text;
                                return dictionary;
                            });
                        }

                        _isInitialize = true;
                    }                    
                }
               
            }
        }

        /// <summary>
        /// Clears the cache, this is used for cache refreshers to ensure that the cache is up to date across all servers 
        /// </summary>
        internal static void ClearCache()
        {
            Items.Clear();
            //reset the flag so that we re-lookup the cache
            _isInitialize = false;
        }

        /// <summary>
        /// Retrieves the value of a languagetranslated item given the key
        /// </summary>
        /// <param name="key">Unique identifier</param>
        /// <param name="languageId">Umbraco languageid</param>
        /// <returns>The language translated text</returns>
        public static string Text(Guid key, int languageId)
        {
            EnsureCache();

            Dictionary<int, string> val;
            if (Items.TryGetValue(key, out val))
            {
                return val[languageId];
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
            EnsureCache();

            Dictionary<int, string> val;
            if (Items.TryGetValue(key, out val))
            {
                return val.ContainsKey(languageId);
            }
            return false;
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
            if (!hasText(key, languageId)) throw new ArgumentException("Key does not exist");
            
            ApplicationContext.Current.DatabaseContext.Database.Update<LanguageTextDto>(
                string.Format("set {0} = @value where LanguageId = @languageId And UniqueId = @key",
                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("value")),
                new {value = value, languageId = languageId, key = key});
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
            if (hasText(key, languageId)) throw new ArgumentException("Key being add'ed already exists");

            ApplicationContext.Current.DatabaseContext.Database.Insert(new LanguageTextDto
                                                                       {
                                                                           LanguageId = languageId,
                                                                           Value = value,
                                                                           UniqueId = key
                                                                       });
        }
        
        /// <summary>
        /// Removes all languagetranslated texts associated to the unique identifier.
        /// </summary>
        /// <param name="key">Unique identifier</param>
        public static void removeText(Guid key)
        {
            // remove from database
            ApplicationContext.Current.DatabaseContext.Database.Delete<LanguageTextDto>("where UniqueId =  @UniqueId", new { UniqueId = key });
        }

        /// <summary>
        /// Removes all entries by language id.
        /// Primary used when deleting a language from Umbraco.
        /// </summary>
        /// <param name="languageId"></param>
        [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
        public static void RemoveByLanguage(int languageId)
        {
            // remove from database
            ApplicationContext.Current.DatabaseContext.Database.Delete<LanguageTextDto>("where languageId =  @languageId", new { languageId = languageId });
        }
    }
}