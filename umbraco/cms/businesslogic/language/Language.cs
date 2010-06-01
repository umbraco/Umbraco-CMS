using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Xml;
using umbraco.cms.businesslogic.cache;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Umbraco.Test")]

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
        #region Private members
        private int _id;
        private string _name = "";
        private string _friendlyName;
        private string _cultureAlias; 
        #endregion

        #region Constants and static members

        private static object getLanguageSyncLock = new object();
        private static readonly string UmbracoLanguageCacheKey = "UmbracoPropertyTypeCache";

        private const int DefaultLanguageId = 1;

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        protected internal const string m_SQLOptimizedGetAll = @"select * from umbracoLanguage";

        private static readonly object m_Locker = new object();

        #endregion
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Language"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public Language(int id)
        {
            var lang = GetAllAsList()
                .Where(x => x.id == id)
                .SingleOrDefault();
            if (lang == null)
            {
                throw new ArgumentException("No language found with the specified id");
            }
            
            _id = lang.id;
            _cultureAlias = lang.CultureAlias;
            _friendlyName = lang.FriendlyName;
        }

        /// <summary>
        /// Empty constructor used to create a language object manually
        /// </summary>
        internal Language() { }

        #endregion

        #region Static methods

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

                InvalidateCache();

                NewEventArgs e = new NewEventArgs();
                GetByCultureCode(CultureCode).OnNew(e);
            }
        }

        private static void InvalidateCache()
        {
            Cache.ClearCacheItem(UmbracoLanguageCacheKey);
        }

        /// <summary>
        /// Method for accessing all installed languagess
        /// </summary>
        [Obsolete("Use the GetAllAsList() method instead")]
        public static Language[] getAll
        {
            get
            {
                return GetAllAsList().ToArray();
            }
        }

        /// <summary>
        /// Returns all installed languages
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will return a cached set of all languages. if the cache is not found it will create it.
        /// </remarks>
        public static IEnumerable<Language> GetAllAsList()
        {
            return Cache.GetCacheItem<IEnumerable<Language>>(UmbracoLanguageCacheKey, getLanguageSyncLock, TimeSpan.FromMinutes(60),
                delegate
                {
                    var languages = new List<Language>();

                    using (IRecordsReader dr = SqlHelper.ExecuteReader(m_SQLOptimizedGetAll))
                    {
                        while (dr.Read())
                        {
                            //create the ContentType object without setting up
                            Language ct = new Language();
                            ct.PopulateFromReader(dr);
                            languages.Add(ct);
                        }
                    }
                    return languages;
                });
        }

        /// <summary>
        /// Gets the language by its culture code, if no language is found, null is returned
        /// </summary>
        /// <param name="CultureCode">The culture code.</param>
        /// <returns></returns>
        public static Language GetByCultureCode(String cultureCode)
        {
            if (new CultureInfo(cultureCode) != null)
            {
                return GetAllAsList()
                        .Where(x => x.CultureAlias == cultureCode)
                        .SingleOrDefault();
            }

            return null;
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

        #endregion

        #region Public Properties
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
        #endregion

        #region Public methods

        /// <summary>
        /// Ensures uniqueness by id
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var l = obj as Language;
            if (l != null)
            {
                return id.Equals(l.id);
            }
            return false;
        }

        /// <summary>
        /// Ensures uniqueness by id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id.GetHashCode();
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
                InvalidateCache();
                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Deletes the current Language.
        /// 
        /// Notice: this can have various sideeffects - use with care.
        /// </summary>
        /// <remarks>
        /// You cannot delete the default language: en-US, this is installed by default and is required.
        /// </remarks>
        public void Delete()
        {
            

            if (this.id == DefaultLanguageId)
            {
                throw new InvalidOperationException("You cannot delete the default language: en-US");
            }


            if (SqlHelper.ExecuteScalar<int>("SELECT count(id) FROM umbracoDomains where domainDefaultLanguage = @id",
                SqlHelper.CreateParameter("@id", id)) == 0)
            {

                DeleteEventArgs e = new DeleteEventArgs();
                FireBeforeDelete(e);

                if (!e.Cancel)
                {
                    //remove the dictionary entries first
                    Item.RemoveByLanguage(id);

                    InvalidateCache();

                    SqlHelper.ExecuteNonQuery("delete from umbracoLanguage where id = @id",
                        SqlHelper.CreateParameter("@id", id));
                    
                    FireAfterDelete(e);
                }
            }
            else
            {
                Log.Add(LogTypes.Error, umbraco.BasePages.UmbracoEnsuredPage.CurrentUser, -1, "Could not remove Language " + _friendlyName + " because it's attached to a node");
                throw new DataException("Cannot remove language " + _friendlyName + " because it's attached to a domain on a node");                
            }
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
        #endregion

        #region Protected methods

        protected void PopulateFromReader(IRecordsReader dr)
        {
            _id = Convert.ToInt32(dr.GetShort("id"));
            _cultureAlias = dr.GetString("languageISOCode");
            
            updateNames();
        } 
        #endregion

        #region Private methods

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

        #endregion

        #region Events

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
        #endregion
    }
}