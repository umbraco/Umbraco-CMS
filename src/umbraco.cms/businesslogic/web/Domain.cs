using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.language;
using umbraco.interfaces;
using umbraco.DataLayer;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("umbraco.Test")]

namespace umbraco.cms.businesslogic.web
{
    /// <summary>
    /// Summary description for Domain.
    /// </summary>
    public class Domain
    {
        private Language _language;
        private string _name;
        private int _root;
        private int _id;

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		public Domain(int Id)
        {
            initDomain(Id);
        }

        public Domain(string DomainName)
        {
            object result = SqlHelper.ExecuteScalar<object>(
				"select id from umbracoDomains where domainName = @DomainName",
				SqlHelper.CreateParameter("@DomainName", DomainName));
            if (result == null || !(result is int))
                throw new Exception(string.Format("Domain name '{0}' does not exists", DomainName));
            initDomain((int) result);
        }

        private void initDomain(int id)
        {
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
				"select domainDefaultLanguage, domainRootStructureID, domainName from umbracoDomains where id = @ID",
				SqlHelper.CreateParameter("@ID", id)))
            {
                if (dr.Read())
                {
                    _id = id;
                    _language = new Language(dr.GetInt("domainDefaultLanguage"));
                    _name = dr.GetString("domainName");
                    _root = dr.GetInt("domainRootStructureID");
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                SqlHelper.ExecuteNonQuery("update umbracoDomains set domainName = @domainName where id = @id",
                                          SqlHelper.CreateParameter("@domainName", value),
                                          SqlHelper.CreateParameter("@id",_id));
                _name = value;
            }
        }

        public Language Language
        {
            get { return _language; }
            set
            {
                SqlHelper.ExecuteNonQuery("update umbracoDomains set domainDefaultLanguage = @language where id = @id",
                                          SqlHelper.CreateParameter("@language", value.id),
                                          SqlHelper.CreateParameter("@id", _id));
                _language = value;
            }
        }

        public int RootNodeId
        {
            get { return _root; }
            set
            {
                SqlHelper.ExecuteNonQuery(string.Format(
                                            "update umbracoDomains set domainRootStructureID = '{0}' where id = {1}",
                                            value, _id));
                _root = value;
            }
        }

        public int Id
        {
            get { return _id; }
        }

        public void Delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel) {
                SqlHelper.ExecuteNonQuery(string.Format("delete from umbracoDomains where id = {0}", Id));
                InvalidateCache();

                FireAfterDelete(e);
            }
        }

        public void Save(){
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) {
                InvalidateCache();

                //Add save method here at some point so this actually does something besides fire events... 

                FireAfterSave(e);
            }
        }

        #region Statics

		private static void InvalidateCache()
		{
			Cache.ClearCacheItem("UmbracoDomainList");
		}

		private static readonly object getDomainsSyncLock = new object();
        
        internal static List<Domain> GetDomains()
        {
			return Cache.GetCacheItem<List<Domain>>("UmbracoDomainList", getDomainsSyncLock, TimeSpan.FromMinutes(30),
        		delegate
        		{
        			List<Domain> result = new List<Domain>();
        			using(IRecordsReader dr = SqlHelper.ExecuteReader(
                                                            "select id, domainName from umbracoDomains"))
        			{
        				while(dr.Read())
        				{
        					string domainName = dr.GetString("domainName");
        					int domainId = dr.GetInt("id");

							if (result.Find(delegate(Domain d) { return d.Name == domainName; }) == null)
								result.Add(new Domain(domainId));
							else
        						Log.Add(LogTypes.Error, User.GetUser(0), -1,
        							string.Format("Domain already exists in list ({0})", domainName));
        				}
        			}
        			return result;
        		});
        }

        public static Domain GetDomain(string DomainName)
        {
        	return GetDomains().Find(delegate(Domain d) { return d.Name == DomainName; });
        }

        public static int GetRootFromDomain(string DomainName)
        {
        	Domain d = GetDomain(DomainName);
			if (d == null) return -1;
        	return d._root;
        }

        public static Domain[] GetDomainsById(int nodeId)
        {
			return GetDomains().FindAll(delegate(Domain d) { return d._root == nodeId; }).ToArray();
        }

        public static bool Exists(string DomainName)
        {
			return GetDomain(DomainName) != null;
        }

        public static void MakeNew(string DomainName, int RootNodeId, int LanguageId)
        {
            if (Exists(DomainName.ToLower()))
                throw new Exception("Domain " + DomainName + " already exists!");
            else
            {
                //need to check if the language exists first
                if (Language.GetAllAsList().Where(x => x.id == LanguageId).SingleOrDefault() == null)
                {
                    throw new ArgumentException("No language exists for the LanguageId specified");
                }
                
                SqlHelper.ExecuteNonQuery("insert into umbracoDomains (domainDefaultLanguage, domainRootStructureID, domainName) values (@domainDefaultLanguage, @domainRootStructureID, @domainName)",
                                          SqlHelper.CreateParameter("@domainDefaultLanguage", LanguageId),
                                          SqlHelper.CreateParameter("@domainRootStructureID", RootNodeId),
                                          SqlHelper.CreateParameter("@domainName", DomainName.ToLower()));

            	InvalidateCache();
                NewEventArgs e = new NewEventArgs();
                new Domain(DomainName).OnNew(e);
            }

            

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
    }
}