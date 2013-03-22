using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.language
{
    /// <summary>
    /// Item class contains method for accessing language translated text, its a generic component which
    /// can be used for storing language translated content, all items are associated to an unique identifier (Guid)
    /// 
    /// The data is cached and are usable in the public website.
    /// 
    /// Primarily used by the built-in dictionary
    /// 
    /// </summary>
    public class Item
    {
        private static readonly ConcurrentDictionary<Guid, Dictionary<int, string>> Items = new ConcurrentDictionary<Guid, Dictionary<int, string>>();        
        private static volatile bool _isInitialize;
        private static readonly object Locker = new object();

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Populates the global hash table with the data from the database.
        /// </summary>
        private static void EnsureData()
        {
            if (!_isInitialize)
            {
                lock (Locker)
                {
                    //double check
                    if (!_isInitialize)
                    {
                        // load all data
                        using (IRecordsReader dr = SqlHelper.ExecuteReader("Select LanguageId, UniqueId,[value] from cmsLanguageText order by UniqueId"))
                        {
                            while (dr.Read())
                            {
                                var languageId = dr.GetInt("LanguageId");
                                var uniqueId = dr.GetGuid("UniqueId");
                                var text = dr.GetString("value");

                                UpdateCache(languageId, uniqueId, text);
                            }
                        }                        
                        _isInitialize = true;
                    }                    
                }
               
            }
        }

        private static void UpdateCache(int languageId, Guid key, string text)
        {
            Items.AddOrUpdate(key, guid =>
                {
                    var languagevalues = new Dictionary<int, string> {{languageId, text}};
                    return languagevalues;
                }, (guid, dictionary) =>
                    {
                        // add/update the text for the id
                        dictionary[languageId] = text;
                        return dictionary;
                    });
        }
        
        /// <summary>
        /// Retrieves the value of a languagetranslated item given the key
        /// </summary>
        /// <param name="key">Unique identifier</param>
        /// <param name="languageId">Umbraco languageid</param>
        /// <returns>The language translated text</returns>
        public static string Text(Guid key, int languageId)
        {
            EnsureData();

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
            EnsureData();

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
            EnsureData();

            if (!hasText(key, languageId)) throw new ArgumentException("Key does not exist");
            
            SqlHelper.ExecuteNonQuery("Update cmsLanguageText set [value] = @value where LanguageId = @languageId And UniqueId = @key",
                SqlHelper.CreateParameter("@value", value),
                SqlHelper.CreateParameter("@languageId", languageId),
                SqlHelper.CreateParameter("@key", key));

            UpdateCache(languageId, key, value);
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
            EnsureData();

            if (hasText(key, languageId)) throw new ArgumentException("Key being add'ed already exists");
            
            SqlHelper.ExecuteNonQuery("Insert Into cmsLanguageText (languageId,UniqueId,[value]) values (@languageId, @key, @value)",
                SqlHelper.CreateParameter("@languageId", languageId),
                SqlHelper.CreateParameter("@key", key),
                SqlHelper.CreateParameter("@value", value));

            UpdateCache(languageId, key, value);
        }
        
        /// <summary>
        /// Removes all languagetranslated texts associated to the unique identifier.
        /// </summary>
        /// <param name="key">Unique identifier</param>
        public static void removeText(Guid key)
        {
            EnsureData();

            // remove from database
            SqlHelper.ExecuteNonQuery("Delete from cmsLanguageText where UniqueId =  @key",
                SqlHelper.CreateParameter("@key", key));

            // remove from cache
            Dictionary<int, string> val;
            Items.TryRemove(key, out val);
        }

        /// <summary>
        /// Removes all entries by language id.
        /// Primary used when deleting a language from Umbraco.
        /// </summary>
        /// <param name="languageId"></param>
        public static void RemoveByLanguage(int languageId)
        {
            EnsureData();

            // remove from database
            SqlHelper.ExecuteNonQuery("Delete from cmsLanguageText where languageId =  @languageId",
                SqlHelper.CreateParameter("@languageId", languageId));

            //we need to lock here because the inner dictionary is not concurrent, seems overkill to have a nested concurrent dictionary
            lock (Locker)
            {
                //delete all of the items belonging to the language
                foreach (var entry in Items.Values)
                {
                    if (entry.ContainsKey(languageId))
                    {
                        entry.Remove(languageId);
                    }
                }    
            }
            
        }
    }
}