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

        private const string CacheKey = "ApplicationCache";
        internal const string AppConfigFileName = "applications.config";
        private static string _appConfig;
        private static readonly object Locker = new object();

        /// <summary>
        /// gets/sets the application.config file path
        /// </summary>
        /// <remarks>
        /// The setter is generally only going to be used in unit tests, otherwise it will attempt to resolve it using the IOHelper.MapPath
        /// </remarks>
        internal static string AppConfigFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_appConfig))
                {
                    _appConfig = IOHelper.MapPath(SystemDirectories.Config + "/" + AppConfigFileName);
                }
                return _appConfig;
            }
            set { _appConfig = value; }
        }

        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        internal static List<Application> Apps
        {
            get
            {
                //Whenever this is accessed, we need to ensure the cache exists!
                EnsureCache();

                return HttpRuntime.Cache[CacheKey] as List<Application>;
            }
            set
            {
                HttpRuntime.Cache.Insert(CacheKey, value);
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
        public static Application getByAlias(string appAlias)
        {
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
            HttpRuntime.Cache.Remove(CacheKey);
            EnsureCache();
        }

        /// <summary>
        /// Read all Application data and store it in cache.
        /// </summary>
        private static void EnsureCache()
        {
            //don't query the database is the cache is not null
            if (HttpRuntime.Cache[CacheKey] != null)
                return;

            try
            {
                var tmp = new List<Application>();

                //using (IRecordsReader dr =
                //    SqlHelper.ExecuteReader("Select appAlias, appIcon, appName from umbracoApp"))
                //{
                //    while (dr.Read())
                //    {
                //        tmp.Add(new Application(dr.GetString("appName"), dr.GetString("appAlias"), dr.GetString("appIcon")));
                //    }
                //}

                LoadXml(doc =>
                {

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

                //TODO: Perhaps we should log something  here??
            }

        }

        internal static void LoadXml(Action<XDocument> callback, bool saveAfterCallback)
        {
            lock (Locker)
            {
                var doc = File.Exists(AppConfigFilePath)
                    ? XDocument.Load(AppConfigFilePath)
                    : XDocument.Parse("<?xml version=\"1.0\"?><applications />");

                if (doc.Root != null)
                {
                    callback.Invoke(doc);

                    if (saveAfterCallback)
                    {
                        //ensure the folder is created!
                        Directory.CreateDirectory(Path.GetDirectoryName(AppConfigFilePath));

                        doc.Save(AppConfigFilePath);

                        ReCache();
                    }
                }
            }
        }
    }
}
