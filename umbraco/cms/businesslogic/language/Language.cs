using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Xml;
using umbraco.cms.businesslogic.cache;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.language
{
    /// <summary>
    /// The language class contains methods for creating and modifing installed languages.
    /// 
    /// A language is used internal in the umbraco console for displaying languagespecific text and 
    /// in the public website for language/country specific representation of ex. date/time, currencies.
    /// 
    /// Besides by using the built in Dictionary you are able to store language specific bits and pieces of translated text
    /// for use in templates.
    /// </summary>
    public class Language
    {
        private int _id;
        private string _name = "";
        private string _friendlyName;
        private string _cultureAlias;


        private static string _ConnString = GlobalSettings.DbDSN;
        private static object getLanguageSyncLock = new object();

        private static readonly string UmbracoLanguageCacheKey = "UmbracoPropertyTypeCache";


        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Language"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public Language(int id)
        {
            _id = id;
            _cultureAlias = Cache.GetCacheItem<string>("UmbracoLanguage" + id, getLanguageSyncLock, TimeSpan.FromMinutes(60),
                delegate
                {
                    return SqlHelper.ExecuteScalar<string>(
                        "select languageISOCode from umbracoLanguage where id = @LanguageId", SqlHelper.CreateParameter("@LanguageId", id));
                });

            updateNames();
        }



        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public virtual void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                Cache.ClearCacheItem("UmbracoLanguage" + id);
                FireAfterSave(e);
            }
        }

        private void updateNames()
        {
            try
            {
                CultureInfo ci = new CultureInfo(_cultureAlias);
                _friendlyName = ci.DisplayName;
            }
            catch
            {
                _friendlyName = _name + "(unknown Culture)";
            }
        }

        /// <summary>
        /// Creates a new language given the culture code - ie. da-dk  (denmark)
        /// </summary>
        /// <param name="CultureCode">Culturecode of the language</param>
        public static void MakeNew(string CultureCode)
        {
            if (new CultureInfo(CultureCode) != null)
            {
                SqlHelper.ExecuteNonQuery(
                    "insert into umbracoLanguage (languageISOCode) values (@CultureCode)",
                    SqlHelper.CreateParameter("@CultureCode", CultureCode));

                NewEventArgs e = new NewEventArgs();
                GetByCultureCode(CultureCode).OnNew(e);

            }
        }

        /// <summary>
        /// Deletes the current Language.
        /// 
        /// Notice: this can have various sideeffects - use with care.
        /// </summary>
        public void Delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                Cache.ClearCacheItem("UmbracoLanguage" + id);
                SqlHelper.ExecuteNonQuery("delete from umbracoLanguage where id = @id",
                    SqlHelper.CreateParameter("@id", id));
                FireAfterDelete(e);
            }
        }

        /// <summary>
        /// Method for accessing all installed languagess
        /// </summary>
        public static Language[] getAll
        {
            get
            {
                List<Language> tmp = new List<Language>();

                using (IRecordsReader dr = SqlHelper.ExecuteReader("select id from umbracoLanguage"))
                {
                    while (dr.Read())
                        tmp.Add(new Language(dr.GetShort("id")));
                }

                return tmp.ToArray();
            }
        }

        /// <summary>
        /// Gets the language by its culture code, if no language is found, null is returned
        /// </summary>
        /// <param name="CultureCode">The culture code.</param>
        /// <returns></returns>
        public static Language GetByCultureCode(String CultureCode)
        {
            if (new CultureInfo(CultureCode) != null)
            {
                return Cache.GetCacheItem<Language>(GetCacheKeyByCultureAlias(CultureCode), getLanguageSyncLock,
                TimeSpan.FromHours(6),
                delegate
                {
                    try
                    {
                        object sqlLangId =
                            SqlHelper.ExecuteScalar<object>(
                                "select id from umbracoLanguage where languageISOCode = @CultureCode",
                                SqlHelper.CreateParameter("@CultureCode", CultureCode));
                        if (sqlLangId != null)
                        {
                            int langId;
                            if (int.TryParse(sqlLangId.ToString(), out langId))
                            {
                                return new Language(langId);
                            }
                        }
                        return null;
                    }
                    catch
                    {
                        return null;
                    }
                });
            }

            return null;
        }

        /// <summary>
        /// The id used by umbraco to identify the language
        /// </summary>
        public int id
        {
            get { return _id; }
        }

        /// <summary>
        /// The culture code of the language: ie. Danish/Denmark da-dk
        /// </summary>
        public string CultureAlias
        {
            get { return _cultureAlias; }
            set
            {
                _cultureAlias = value;
                SqlHelper.ExecuteNonQuery(
                    "update umbracoLanguage set languageISOCode = @cultureAlias where id = @id", SqlHelper.CreateParameter("@id", id),
                    SqlHelper.CreateParameter("@cultureAlias", _cultureAlias));
                updateNames();
            }
        }

        /// <summary>
        /// The user friendly name of the language/country
        /// </summary>
        public string FriendlyName
        {
            get { return _friendlyName; }
        }

        /// <summary>
        /// Converts the instance to XML
        /// </summary>
        /// <param name="xd">The xml document.</param>
        /// <returns></returns>
        public System.Xml.XmlNode ToXml(XmlDocument xd)
        {
            XmlNode language = xd.CreateElement("Language");
            language.Attributes.Append(xmlHelper.addAttribute(xd, "Id", this.id.ToString()));
            language.Attributes.Append(xmlHelper.addAttribute(xd, "CultureAlias", this.CultureAlias));
            language.Attributes.Append(xmlHelper.addAttribute(xd, "FriendlyName", this.FriendlyName));

            return language;
        }


        /// <summary>
        /// Imports a language from XML
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public static Language Import(XmlNode xmlData)
        {
            string cA = xmlData.Attributes["CultureAlias"].Value;
            if (Language.GetByCultureCode(cA) == null)
            {
                Language.MakeNew(cA);
                return Language.GetByCultureCode(cA);
            }
            else
            {
                return null;
            }
        }


        private void InvalidateCache()
        {
            Cache.ClearCacheItem(GetCacheKey(this.id));
            Cache.ClearCacheItem(GetCacheKeyByCultureAlias(this.CultureAlias));
        }

        private static string GetCacheKey(int id)
        {
            return UmbracoLanguageCacheKey + id;
        }

        private static string GetCacheKeyByCultureAlias(string cultureAlias)
        {
            return UmbracoLanguageCacheKey + cultureAlias;
        }

        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(Language sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(Language sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(Language sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when a language is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
    }
}