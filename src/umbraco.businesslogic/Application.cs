using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using umbraco.DataLayer;
using umbraco.IO;
using umbraco.interfaces;
using umbraco.BusinessLogic.Utils;
using System.Runtime.CompilerServices;
using umbraco.businesslogic;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Class for handling all registered applications in Umbraco.
    /// </summary>
    public class Application
    {
        private static ISqlHelper _sqlHelper;               

        private const string CACHE_KEY = "ApplicationCache";

        private static readonly string _appConfig =
            IOHelper.MapPath(SystemDirectories.Config + "/applications.config");

        private static readonly object m_Locker = new object();

        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        internal static List<Application> Apps
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
        private int _sortOrder;


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
            //RegisterIApplications();
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
            : this(name, alias, icon, 0)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="sortOrder">The sort order.</param>
        public Application(string name, string alias, string icon, int sortOrder)
        {
            this.name = name;
            this.alias = alias;
            this.icon = icon;
            this.sortOrder = sortOrder;
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
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int sortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
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
            MakeNew(name, alias, icon, Apps.Max(x => x.sortOrder) + 1);
        }

        /// <summary>
        /// Makes the new.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="sortOrder">The sort order.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeNew(string name, string alias, string icon, int sortOrder)
        {
            bool exist = false;
            foreach (Application app in getAll())
            {
                if (app.alias == alias)
                    exist = true;
            }

            if (!exist)
            {
//                SqlHelper.ExecuteNonQuery(@"
//				insert into umbracoApp 
//				(appAlias,appIcon,appName, sortOrder) 
//				values (@alias,@icon,@name,@sortOrder)",
//                SqlHelper.CreateParameter("@alias", alias),
//                SqlHelper.CreateParameter("@icon", icon),
//                SqlHelper.CreateParameter("@name", name),
//                SqlHelper.CreateParameter("@sortOrder", sortOrder));

                LoadXml(doc =>
                {
                    doc.Root.Add(new XElement("add",
                        new XAttribute("alias", alias),
                        new XAttribute("name", name),
                        new XAttribute("icon", icon),
                        new XAttribute("sortOrder", sortOrder)));
                }, true);
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
            return Apps.Find(t => t.alias == appAlias);
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

            //SqlHelper.ExecuteNonQuery("delete from umbracoApp where appAlias = @appAlias",
            //    SqlHelper.CreateParameter("@appAlias", this._alias));

            LoadXml(doc =>
            {
                doc.Root.Elements("add").Where(x => x.Attribute("alias") != null && x.Attribute("alias").Value == this.alias).Remove();
            }, true);
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
        [Obsolete("RegisterIApplications has been depricated. Please use ApplicationStartupHandler.RegisterHandlers() instead.")]
        public static void RegisterIApplications()
        {
            ApplicationStartupHandler.RegisterHandlers();
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

                //using (IRecordsReader dr =
                //    SqlHelper.ExecuteReader("Select appAlias, appIcon, appName from umbracoApp"))
                //{
                //    while (dr.Read())
                //    {
                //        tmp.Add(new Application(dr.GetString("appName"), dr.GetString("appAlias"), dr.GetString("appIcon")));
                //    }
                //}

                LoadXml(doc => {

                    foreach (var addElement in doc.Root.Elements("add").OrderBy(x =>
                        {
                            var sortOrderAttr = x.Attribute("sortOrder");
                            return sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0;
                        }))
                    {
                        var sortOrderAttr = addElement.Attribute("sortOrder");
                        tmp.Add(new Application(addElement.Attribute("name").Value,
                            addElement.Attribute("alias").Value,
                            addElement.Attribute("icon").Value,
                            sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0));
                    }

                }, false);

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

        internal static void LoadXml(Action<XDocument> callback, bool saveAfterCallback)
        {
            lock (m_Locker)
            {
                var doc = File.Exists(_appConfig)
                    ? XDocument.Load(_appConfig)
                    : XDocument.Parse("<?xml version=\"1.0\"?><applications />");

                if (doc.Root != null)
                {
                    callback.Invoke(doc);

                    if (saveAfterCallback)
                    {
                        doc.Save(_appConfig);

                        ReCache();
                    }
                }
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

    public class ApplicationRegistrar : ApplicationStartupHandler
    {
        private ISqlHelper _sqlHelper;
        protected ISqlHelper SqlHelper
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

        public ApplicationRegistrar()
        {
            // Load all Applications by attribute and add them to the XML config
            var types = TypeFinder.FindClassesOfType<IApplication>()
                .Where(x => x.GetCustomAttributes(typeof(ApplicationAttribute), false).Any());

            var attrs = types.Select(x => (ApplicationAttribute)x.GetCustomAttributes(typeof(ApplicationAttribute), false).Single())
                .Where(x => Application.getByAlias(x.Alias) == null);

            var allAliases = Application.getAll().Select(x => x.alias).Concat(attrs.Select(x => x.Alias));
            var inString = "'" + string.Join("','", allAliases) + "'";

            Application.LoadXml(doc =>
            {
                foreach (var attr in attrs)
                {
                    doc.Root.Add(new XElement("add",
                        new XAttribute("alias", attr.Alias),
                        new XAttribute("name", attr.Name),
                        new XAttribute("icon", attr.Icon),
                        new XAttribute("sortOrder", attr.SortOrder)));
                }

                var dbApps = SqlHelper.ExecuteReader("SELECT * FROM umbracoApp WHERE appAlias NOT IN ("+ inString +")");
                while (dbApps.Read())
                {
                    doc.Root.Add(new XElement("add",
                        new XAttribute("alias", dbApps.GetString("appAlias")),
                        new XAttribute("name", dbApps.GetString("appName")),
                        new XAttribute("icon", dbApps.GetString("appIcon")),
                        new XAttribute("sortOrder", dbApps.GetByte("sortOrder"))));
                }

            }, true);

            //SqlHelper.ExecuteNonQuery("DELETE FROM umbracoApp");
        }
    }
}
