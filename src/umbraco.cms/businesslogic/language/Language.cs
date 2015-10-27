using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Services;
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
    [Obsolete("Use the LocalizationService instead")]
    public class Language
    {
        #region Private members

        internal ILanguage LanguageEntity { get; private set; }

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
            LanguageEntity = ApplicationContext.Current.Services.LocalizationService.GetLanguageById(id);
            if (LanguageEntity == null)
            {
                throw new ArgumentException("No language found with the specified id");
            }
        }

        /// <summary>
        /// Empty constructor used to create a language object manually
        /// </summary>
        internal Language() { }

        internal Language(ILanguage langEntity)
        {
            LanguageEntity = langEntity;
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Creates a new language given the culture code - ie. da-dk  (denmark)
        /// </summary>
        /// <param name="cultureCode">Culturecode of the language</param>
        public static void MakeNew(string cultureCode)
        {
            var culture = GetCulture(cultureCode);
            if (culture != null)
            {
                //insert it
                var lang = new Umbraco.Core.Models.Language(cultureCode)
                {
                    CultureName = culture.DisplayName
                };
                ApplicationContext.Current.Services.LocalizationService.Save(lang);
                var ct = new Language(lang);
                ct.OnNew(new NewEventArgs());
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
            var all = ApplicationContext.Current.Services.LocalizationService.GetAllLanguages().Select(x => new Language(x)).ToArray();
            return all;
        }
      

        /// <summary>
        /// Gets the language by its culture code, if no language is found, null is returned
        /// </summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns></returns>
        public static Language GetByCultureCode(string cultureCode)
        {
            var found = ApplicationContext.Current.Services.LocalizationService.GetLanguageByIsoCode(cultureCode);
            if (found == null) return null;
            var lang = new Language(found);
            return lang;
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
        [Obsolete("This is no longer used and will be removed in future versions")]
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
            get { return LanguageEntity.Id; }
        }

        /// <summary>
        /// The culture code of the language: ie. Danish/Denmark da-dk
        /// </summary>
        public string CultureAlias
        {
            get { return LanguageEntity.IsoCode; }
            set
            {
                LanguageEntity.IsoCode = value;
            }
        }

        /// <summary>
        /// The user friendly name of the language/country
        /// </summary>
        public string FriendlyName
        {
            get { return LanguageEntity.CultureInfo.DisplayName; }
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
        /// Used to persist object changes to the database
        /// </summary>
        public virtual void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            //Do the update!
            ApplicationContext.Current.Services.LocalizationService.Save(LanguageEntity);

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
            if (ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>("SELECT count(id) FROM umbracoDomains where domainDefaultLanguage = @id", new { id = id }) == 0)
            {
                var e = new DeleteEventArgs();
                FireBeforeDelete(e);

                if (!e.Cancel)
                {
                    ApplicationContext.Current.Services.LocalizationService.Delete(LanguageEntity);

                    FireAfterDelete(e);
                }
            }
            else
            {
                var e = new DataException("Cannot remove language " + LanguageEntity.CultureInfo.DisplayName + " because it's attached to a domain on a node");
                LogHelper.Error<Language>("Cannot remove language " + LanguageEntity.CultureInfo.DisplayName + " because it's attached to a domain on a node", e);
                throw e;
            }           
        }

        /// <summary>
        /// Converts the instance to XML
        /// </summary>
        /// <param name="xd">The xml document.</param>
        /// <returns></returns>
        public XmlNode ToXml(XmlDocument xd)
        {
            var serializer = new EntityXmlSerializer();
            var xml = serializer.Serialize(LanguageEntity);
            return xml.GetXmlNode(xd);
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