using System;
using System.Collections;
using System.Data;

using umbraco.DataLayer;
using umbraco.BusinessLogic;

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
        private static bool isInitialized = false;
        private static string _Connstring = GlobalSettings.DbDSN;

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        private static void ensureData()
        {
            if (!isInitialized)
            {
                // load all data
                IRecordsReader dr = SqlHelper.ExecuteReader("Select LanguageId, UniqueId,[value] from cmsLanguageText order by UniqueId");

                while (dr.Read())
                {
                    int LanguageId = dr.GetInt("LanguageId");
                    Guid UniqueId = dr.GetGuid("UniqueId");
                    string text = dr.GetString("value");
                    updateCache(LanguageId, UniqueId, text);
                }
                isInitialized = true;
                dr.Close();
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

            updateCache(LanguageId, key, value);
            SqlHelper.ExecuteNonQuery("Update cmsLanguageText set [value] = @value where LanguageId = @languageId And UniqueId = @key",
                SqlHelper.CreateParameter("@value", value),
                SqlHelper.CreateParameter("@languageId", LanguageId),
                SqlHelper.CreateParameter("@key", key));
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
            updateCache(LanguageId, key, value);
            SqlHelper.ExecuteNonQuery("Insert Into cmsLanguageText (languageId,UniqueId,[value]) values (@languageId, @key, @value)",
                SqlHelper.CreateParameter("@languageId", LanguageId),
                SqlHelper.CreateParameter("@key", key),
                SqlHelper.CreateParameter("@value", value));
        }
        /// <summary>
        /// Removes all languagetranslated texts associated to the unique identifier.
        /// </summary>
        /// <param name="key">Unique identifier</param>
        public static void removeText(Guid key)
        {
            // remove from cache
            _items.Remove(key);
            // remove from database
            SqlHelper.ExecuteNonQuery("Delete from cmsLanguageText where UniqueId =  @key",
                SqlHelper.CreateParameter("@key", key));
        }
    }
}