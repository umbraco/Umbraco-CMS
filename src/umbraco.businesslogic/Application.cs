using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.BusinessLogic.Utils;
using System.Runtime.CompilerServices;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Class for handling all registered applications in Umbraco.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Applications found through reflection
        /// </summary>
        private static readonly List<IApplication> _applications = new List<IApplication>();
        
        private static ISqlHelper _sqlHelper;               

        private const string CACHE_KEY = "ApplicationCache";

        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        private static List<Application> Apps
        {
            get
            {                
                //ensure cache exists
                if (HttpRuntime.Cache[CACHE_KEY] == null)
                    ReCache();
                return HttpRuntime.Cache[CACHE_KEY] as List<Application>;
            }
            set
            {
                HttpRuntime.Cache.Insert(CACHE_KEY, value);
            }
        }

        private string _name;
        private string _alias;
        private string _icon;


        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        public static ISqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                {
                    try
                    {
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN);
                    }
                    catch { }
                }
                return _sqlHelper;
            }
        }

        /// <summary>
        /// A static constructor that will cache all application trees
        /// </summary>
        static Application()
        {
            RegisterIApplications();
            Cache();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="name">The application name.</param>
        /// <param name="alias">The application alias.</param>
        /// <param name="icon">The application icon.</param>
        public Application(string name, string alias, string icon)
        {
            this.name = name;
            this.alias = alias;
            this.icon = icon;
        }

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        /// <value>The name.</value>
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the application alias.
        /// </summary>
        /// <value>The alias.</value>
        public string alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        /// <summary>
        /// Gets or sets the application icon.
        /// </summary>
        /// <value>The application icon.</value>
        public string icon
        {
            get { return _icon; }
            set { _icon = value; }
        }        

        /// <summary>
        /// Creates a new applcation if no application with the specified alias is found.
        /// </summary>
        /// <param name="name">The application name.</param>
        /// <param name="alias">The application alias.</param>
        /// <param name="icon">The application icon, which has to be located in umbraco/images/tray folder.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeNew(string name, string alias, string icon)
        {
            bool exist = false;
            foreach (Application app in getAll())
            {
                if (app.alias == alias)
                    exist = true;
            }

            if (!exist)
            {
                int sortOrder = (1 + SqlHelper.ExecuteScalar<int>("SELECT MAX(sortOrder) FROM umbracoApp"));

                SqlHelper.ExecuteNonQuery(@"
				insert into umbracoApp 
				(appAlias,appIcon,appName, sortOrder) 
				values (@alias,@icon,@name,@sortOrder)",
                SqlHelper.CreateParameter("@alias", alias),
                SqlHelper.CreateParameter("@icon", icon),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@sortOrder", sortOrder));

                ReCache();
            }

        }


        //public static void MakeNew(IApplication Iapp, bool installAppTrees) {

        //    MakeNew(Iapp.Name, Iapp.Alias, Iapp.Icon);

        //    if (installAppTrees) {
                
        //    }
        //}


        /// <summary>
        /// Gets the application by its alias.
        /// </summary>
        /// <param name="appAlias">The application alias.</param>
        /// <returns></returns>
        public static Application getByAlias(string appAlias) {
            return Apps.Find(
                delegate(Application t) {
                    return (t.alias == appAlias);
                }
            );

        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete() 
        {
            //delete the assigned applications
            SqlHelper.ExecuteNonQuery("delete from umbracoUser2App where app = @appAlias", SqlHelper.CreateParameter("@appAlias", this.alias));

            //delete the assigned trees
            var trees = ApplicationTree.getApplicationTree(this.alias);
            foreach (var t in trees)
            {
                t.Delete();
            }

            SqlHelper.ExecuteNonQuery("delete from umbracoApp where appAlias = @appAlias",
                SqlHelper.CreateParameter("@appAlias", this._alias));

            ReCache();
        }

        /// <summary>
        /// Gets all applications registered in umbraco from the umbracoApp table..
        /// </summary>
        /// <returns>Returns a Application Array</returns>
        public static List<Application> getAll()
        {
            return Apps;
        }

        /// <summary>
        /// Stores all references to classes that are of type IApplication
        /// </summary>
        public static void RegisterIApplications()
        {
            if (GlobalSettings.Configured) {
                
                List<Type> types = TypeFinder.FindClassesOfType<IApplication>();
                
                foreach (Type t in types) {
                    try
                    {
                        IApplication typeInstance = Activator.CreateInstance(t) as IApplication;
                        if (typeInstance != null)
                        {
                            _applications.Add(typeInstance);

                            if (HttpContext.Current != null)
                                HttpContext.Current.Trace.Write("registerIapplications", " + Adding application '" + typeInstance.Alias);
                        }
                    }
                    catch (Exception ee) {
                        Log.Add(LogTypes.Error, -1, "Error loading IApplication: " + ee.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Removes the Application cache and re-reads the data from the db.
        /// </summary>
        private static void ReCache()
        {
            HttpRuntime.Cache.Remove(CACHE_KEY);
            Cache();
        }

        /// <summary>
        /// Read all Application data and store it in cache.
        /// </summary>
        private static void Cache()
        {
            //don't query the database is the cache is not null
            if (HttpRuntime.Cache[CACHE_KEY] != null)
                return;

            try
            {
                List<Application> tmp = new List<Application>();

                using (IRecordsReader dr =
                    SqlHelper.ExecuteReader("Select appAlias, appIcon, appName from umbracoApp"))
                {
                    while (dr.Read())
                    {
                        tmp.Add(new Application(dr.GetString("appName"), dr.GetString("appAlias"), dr.GetString("appIcon")));
                    }
                }

                Apps = tmp;
            }
            catch
            {
                //this is a bit of a hack that just ensures the application doesn't crash when the
                //installer is run and there is no database or connection string defined.
                //the reason this method may get called during the installation is that the 
                //SqlHelper of this class is shared amongst everything "Application" wide.
            }

        }
    }

    public enum DefaultApps
    {
        content,
        media,
        users,
        settings,
        developer,
        member,
        translation
    }
}
