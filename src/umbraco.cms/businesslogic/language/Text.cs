using System;
using System.Collections;
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
        private static Hashtable _items = Hashtable.Synchronized(new Hashtable());
        private static volatile bool m_IsInitialize = false;
        private static readonly object m_Locker = new object();

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
        private static void ensureData()
        {
            if (!m_IsInitialize)
            {
                lock (m_Locker)
                {
                    //double check
                    if (!m_IsInitialize)
                    {
                        // load all data
                        using (IRecordsReader dr = SqlHelper.ExecuteReader("Select LanguageId, UniqueId,[value] from cmsLanguageText order by UniqueId"))
                        {
                            while (dr.Read())
                            {
                                int LanguageId = dr.GetInt("LanguageId");
                                Guid UniqueId = dr.GetGuid("UniqueId");
                                string text = dr.GetString("value");

                                updateCache(LanguageId, UniqueId, text);
                            }
                        }                        
                        m_IsInitialize = true;
                    }                    
                }
               
            }
        }

        private static void updateCache(int LanguageId, Guid key, string text)
        {
            // test if item already exist in items and update internal data or insert new internal data
            if (_items.ContainsKey(key))
            {
                System.Collections.Hashtable languagevalues = (System.Collections.Hashtable)_items[key];
                
                // check if the current language key is used
                if (languagevalues.ContainsKey(LanguageId))
                {
                    languagevalues[LanguageId] = text;
                }
                else
                {
                    languagevalues.Add(LanguageId, text);
                }
            }
            else
            {
                // insert 
                Hashtable languagevalues = Hashtable.Synchronized(new Hashtable());
                languagevalues.Add(LanguageId, text);
                _items.Add(key, languagevalues);
            }
        }


        /// <summary>
        /// Retrieves the value of a languagetranslated item given the key
        /// </summary>
        /// <param name="key">Unique identifier</param>
        /// <param name="LanguageId">Umbraco languageid</param>
        /// <returns>The language translated text</returns>
        public static string Text(Guid key, int LanguageId)
        {
            ensureData();

            if (hasText(key, LanguageId))
                return ((System.Collections.Hashtable)_items[key])[LanguageId].ToString();
            else
                throw new ArgumentException("Key being requested does not exist");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Unique identifier</param>
        /// <param name="LanguageId">Umbraco language id</param>
        /// <returns>returns True if there is a value associated to the unique identifier with the specified language</returns>
        public static bool hasText(Guid key, int LanguageId)
        {
            ensureData();

            if (_items.ContainsKey(key))
            {
                System.Collections.Hashtable tmp = (System.Collections.Hashtable)_items[key];
                return tmp.ContainsKey(LanguageId);
            }
            return false;
        }
        /// <summary>
        /// Updates the value of the language translated item, throws an exeption if the
        /// key does not exist
        /// </summary>
        /// <param name="LanguageId">Umbraco language id</param>
        /// <param name="key">Unique identifier</param>
        /// <param name="value">The new dictionaryvalue</param>

        public static void setText(int LanguageId, Guid key, string value)
        {
            ensureData();

            if (!hasText(key, LanguageId)) throw new ArgumentException("Key does not exist");
            
            SqlHelper.ExecuteNonQuery("Update cmsLanguageText set [value] = @value where LanguageId = @languageId And UniqueId = @key",
                SqlHelper.CreateParameter("@value", value),
                SqlHelper.CreateParameter("@languageId", LanguageId),
                SqlHelper.CreateParameter("@key", key));

            updateCache(LanguageId, key, value);
        }

        /// <summary>
        /// Adds a new languagetranslated item to the collection
        /// 
        /// </summary>
        /// <param name="LanguageId">Umbraco languageid</param>
        /// <param name="key">Unique identifier</param>
        /// <param name="value"></param>
        public static void addText(int LanguageId, Guid key, string value)
        {
            ensureData();

            if (hasText(key, LanguageId)) throw new ArgumentException("Key being add'ed already exists");
            
            SqlHelper.ExecuteNonQuery("Insert Into cmsLanguageText (languageId,UniqueId,[value]) values (@languageId, @key, @value)",
                SqlHelper.CreateParameter("@languageId", LanguageId),
                SqlHelper.CreateParameter("@key", key),
                SqlHelper.CreateParameter("@value", value));

            updateCache(LanguageId, key, value);
        }
        /// <summary>
        /// Removes all languagetranslated texts associated to the unique identifier.
        /// </summary>
        /// <param name="key">Unique identifier</param>
        public static void removeText(Guid key)
        {
            ensureData();

            // remove from database
            SqlHelper.ExecuteNonQuery("Delete from cmsLanguageText where UniqueId =  @key",
                SqlHelper.CreateParameter("@key", key));

            // remove from cache
            _items.Remove(key);
        }

        /// <summary>
        /// Removes all entries by language id.
        /// Primary used when deleting a language from Umbraco.
        /// </summary>
        /// <param name="languageId"></param>
        public static void RemoveByLanguage(int languageId)
        {
            ensureData();

            // remove from database
            SqlHelper.ExecuteNonQuery("Delete from cmsLanguageText where languageId =  @languageId",
                SqlHelper.CreateParameter("@languageId", languageId));

            //delete all of the items belonging to the language
            foreach (var entry in _items.Values.Cast<Hashtable>())
            {
                if (entry.Contains(languageId))
                {
                    entry.Remove(languageId);
                }
                
            }

        }
    }
}