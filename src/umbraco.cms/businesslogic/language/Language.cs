using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Linq;

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
        private static readonly object Locker = new object();
        #endregion

        #region Constants and static members
        
        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

 
        #endregion
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Language"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public Language(int id)
        {
            var lang = GetAllAsList().SingleOrDefault(x => x.id == id);
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
        /// <param name="cultureCode">Culturecode of the language</param>
        public static void MakeNew(string cultureCode)
        {
            lock (Locker)
            {
                var culture = GetCulture(cultureCode);
                if (culture != null)
                {
                    //insert it
                    var newId = ApplicationContext.Current.DatabaseContext.Database.Insert(new LanguageDto { IsoCode = cultureCode });
                    var ct = new Language { _id = Convert.ToInt32(newId), _cultureAlias = cultureCode };
                    ct.UpdateNames();
                    ct.OnNew(new NewEventArgs());
                     
                }
            }
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
            return ApplicationContext.Current.ApplicationCache.GetCacheItem<IEnumerable<Language>>(
                CacheKeys.LanguageCacheKey,
                TimeSpan.FromMinutes(60),
                () =>
                    {
                        var languages = new List<Language>();
                        var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<LanguageDto>("");
                        foreach (var dto in dtos)
                        {
                            var ct = new Language {_id = dto.Id, _cultureAlias = dto.IsoCode};
                            ct.UpdateNames();
                            languages.Add(ct);
                        }
                        return languages;
                    });
        }

      

        /// <summary>
        /// Gets the language by its culture code, if no language is found, null is returned
        /// </summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns></returns>
        public static Language GetByCultureCode(string cultureCode)
        {
            return GetAllAsList().SingleOrDefault(x => x.CultureAlias == cultureCode);
        }

        private static CultureInfo GetCulture(string cultureCode)
        {
            try
            {
                var culture = new CultureInfo(cultureCode);
                return culture;
            }
            catch (Exception ex)
            {
                LogHelper.Error<Language>("Could not find the culture " + cultureCode, ex);
                return null;
            }       
        }

        /// <summary>
        /// Imports a language from XML
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public static Language Import(XmlNode xmlData)
        {
            var cA = xmlData.Attributes["CultureAlias"].Value;
            if (GetByCultureCode(cA) == null)
            {
                MakeNew(cA);
                return GetByCultureCode(cA);
            }
            return null;
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
                ApplicationContext.Current.DatabaseContext.Database.Update<LanguageDto>(
                    "set languageISOCode = @cultureAlias where id = @id",
                    new { cultureAlias = _cultureAlias,id=id}
                    );
 
                UpdateNames();
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
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
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
            lock (Locker)
            {

                if (ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("SELECT count(id) FROM umbracoDomains where domainDefaultLanguage = @id", new { id = id }) == 0)
                {
                    var e = new DeleteEventArgs();
                    FireBeforeDelete(e);

                    if (!e.Cancel)
                    {
                        //remove the dictionary entries first
                        Item.RemoveByLanguage(id);

                        ApplicationContext.Current.DatabaseContext.Database.Delete<LanguageDto>("where id = @id",
                                                                                                new {id = id});
                        FireAfterDelete(e);
                    }
                }
                else
                {
                    var e = new DataException("Cannot remove language " + _friendlyName + " because it's attached to a domain on a node");
                    LogHelper.Error<Language>("Cannot remove language " + _friendlyName + " because it's attached to a domain on a node", e);
                    throw e;
                }   
            }            
        }

        /// <summary>
        /// Converts the instance to XML
        /// </summary>
        /// <param name="xd">The xml document.</param>
        /// <returns></returns>
        public XmlNode ToXml(XmlDocument xd)
        {
            var language = xd.CreateElement("Language");
            language.Attributes.Append(XmlHelper.AddAttribute(xd, "Id", id.ToString()));
            language.Attributes.Append(XmlHelper.AddAttribute(xd, "CultureAlias", CultureAlias));
            language.Attributes.Append(XmlHelper.AddAttribute(xd, "FriendlyName", FriendlyName));

            return language;
        } 
        #endregion

        #region Protected methods

        protected void PopulateFromReader(IRecordsReader dr)
        {
            _id = Convert.ToInt32(dr.GetShort("id"));
            _cultureAlias = dr.GetString("languageISOCode");
            
            UpdateNames();
        }

        #endregion

        #region Private methods

        private void UpdateNames()
        {
            try
            {
                var ci = new CultureInfo(_cultureAlias);
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