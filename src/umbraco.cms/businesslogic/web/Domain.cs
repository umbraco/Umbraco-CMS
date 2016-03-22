using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
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
            get
            {
                if (DomainEntity.LanguageId.HasValue == false) return null;
                var lang = ApplicationContext.Current.Services.LocalizationService.GetLanguageById(DomainEntity.LanguageId.Value);
                if (lang == null) throw new InvalidOperationException("No language exists with id " + DomainEntity.LanguageId.Value);
                return new Language(lang);
            }
            set
            {
                DomainEntity.LanguageId = value.LanguageEntity.Id;
            }
        }

        public int RootNodeId
        {
            get { return DomainEntity.RootContentId ?? -1; }
            set { DomainEntity.RootContentId = value; }
        }

        public int Id
        {
            get { return DomainEntity.Id; }
        }

        public void Delete()
        {
            ApplicationContext.Current.Services.DomainService.Delete(DomainEntity);

        }

        public void Save()
        {
            ApplicationContext.Current.Services.DomainService.Save(DomainEntity);

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
            return found == null ? -1 : found.RootContentId ?? -1;
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
            var domain = new UmbracoDomain(DomainName)
            {
                RootContentId = RootNodeId,
                LanguageId = LanguageId
            };
            ApplicationContext.Current.Services.DomainService.Save(domain);
        }

        #endregion

      

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