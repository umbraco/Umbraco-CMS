using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.cache;
using Umbraco.Core.Models;
using umbraco.interfaces;
using umbraco.DataLayer;
using System.Runtime.CompilerServices;
using Language = umbraco.cms.businesslogic.language.Language;


namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for Domain.
    /// </summary>
    [Obsolete("Use Umbraco.Core.Models.IDomain and Umbraco.Core.Services.IDomainService instead")]
    public class Domain
    {
        public IDomain DomainEntity { get; set; }
       
		/// <summary>
		/// Empty ctor used for unit tests to create a custom domain
		/// </summary>
		internal Domain()
		{			
		}

        internal Domain(IDomain domain)
        {
            DomainEntity = domain;
        }

        public Domain(int Id)
        {
            DomainEntity = ApplicationContext.Current.Services.DomainService.GetById(Id);
            if (DomainEntity == null)
            {
                throw new Exception(string.Format("Domain name '{0}' does not exists", Id));
            }
        }

        public Domain(string DomainName)
        {
            DomainEntity = ApplicationContext.Current.Services.DomainService.GetByName(DomainName);
            if (DomainEntity == null)
            {
                throw new Exception(string.Format("Domain name '{0}' does not exists", DomainName));
            }
        }

        public string Name
        {
            get { return DomainEntity.DomainName; }
            set { DomainEntity.DomainName = value; }
        }

        public Language Language
        {
            get { return new Language(DomainEntity.Language); }
            set { DomainEntity.Language = value.LanguageEntity; }
        }

        public int RootNodeId
        {
            get { return DomainEntity.RootContent.Id; }
            set
            {
                var content = ApplicationContext.Current.Services.ContentService.GetById(value);
                if (content == null)
                {
                    throw new NullReferenceException("No content found with id " + value);
                }
                DomainEntity.RootContent = content;
            }
        }

        public int Id
        {
            get { return DomainEntity.Id; }
        }

        public void Delete()
        {
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                ApplicationContext.Current.Services.DomainService.Delete(DomainEntity);

                FireAfterDelete(e);
            }
        }

        public void Save(){
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) 
            {
                ApplicationContext.Current.Services.DomainService.Save(DomainEntity);

                FireAfterSave(e);
            }
        }

        #region Statics


        public static IEnumerable<Domain> GetDomains()
        {
            return GetDomains(false);
        }

        public static IEnumerable<Domain> GetDomains(bool includeWildcards)
        {
            return ApplicationContext.Current.Services.DomainService.GetAll(includeWildcards)
                .Select(x => new Domain(x));
        }

        public static Domain GetDomain(string DomainName)
        {
            var found = ApplicationContext.Current.Services.DomainService.GetByName(DomainName);
            return found == null ? null : new Domain(found);
        }

        public static int GetRootFromDomain(string DomainName)
        {
            var found = ApplicationContext.Current.Services.DomainService.GetByName(DomainName);
            return found == null ? -1 : found.RootContent.Id;
        }

        public static Domain[] GetDomainsById(int nodeId)
        {
            return ApplicationContext.Current.Services.DomainService.GetAssignedDomains(nodeId, true)
                .Select(x => new Domain(x))
                .ToArray();
        }

        public static Domain[] GetDomainsById(int nodeId, bool includeWildcards)
        {
            return ApplicationContext.Current.Services.DomainService.GetAssignedDomains(nodeId, includeWildcards)
                .Select(x => new Domain(x))
                .ToArray();
        }

        public static bool Exists(string DomainName)
        {
            return ApplicationContext.Current.Services.DomainService.Exists(DomainName);
        }

        public static void MakeNew(string DomainName, int RootNodeId, int LanguageId)
        {
            var content = ApplicationContext.Current.Services.ContentService.GetById(RootNodeId);
            if (content == null) throw new NullReferenceException("No content exists with id " + RootNodeId);
            var lang = ApplicationContext.Current.Services.LocalizationService.GetLanguageById(LanguageId);
            if (lang == null) throw new NullReferenceException("No language exists with id " + LanguageId);

            var domain = new UmbracoDomain(DomainName)
            {
                RootContent = content,
                Language = lang
            };
            ApplicationContext.Current.Services.DomainService.Save(domain);

            var e = new NewEventArgs();
            var legacyModel = new Domain(domain);
            legacyModel.OnNew(e);
        }

        #endregion

        //EVENTS
        public delegate void SaveEventHandler(Domain sender, SaveEventArgs e);
        public delegate void NewEventHandler(Domain sender, NewEventArgs e);
        public delegate void DeleteEventHandler(Domain sender, DeleteEventArgs e);

        /// <summary>
        /// Occurs when a macro is saved.
        /// </summary>
        public static event SaveEventHandler BeforeSave;
        protected virtual void FireBeforeSave(SaveEventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        public static event SaveEventHandler AfterSave;
        protected virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        public static event NewEventHandler New;
        protected virtual void OnNew(NewEventArgs e) {
            if (New != null)
                New(this, e);
        }

        public static event DeleteEventHandler BeforeDelete;
        protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        public static event DeleteEventHandler AfterDelete;
        protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        #region Pipeline Refactoring

        // NOTE: the wildcard name thing should be managed by the Domain class
        // internally but that would break too much backward compatibility, so
        // we don't do it now. Will do it when the Domain class migrates to the
        // new Core.Models API.

        /// <summary>
        /// Gets a value indicating whether the domain is a wildcard domain.
        /// </summary>
        /// <returns>A value indicating whether the domain is a wildcard domain.</returns>
        public bool IsWildcard
        {
            get { return DomainEntity.IsWildcard; }
        }

        #endregion
    }
}