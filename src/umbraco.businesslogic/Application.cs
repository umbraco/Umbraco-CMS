using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.DataLayer;
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
                return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                    CacheKeys.ApplicationsCacheKey,
                    () =>
                        {
                            ////used for unit tests
                            //if (_testApps != null)
                            //    return _testApps;

                            var tmp = new List<Application>();

                            try
                            {
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
                                return tmp;
                            }
                            catch
                            {
                                //this is a bit of a hack that just ensures the application doesn't crash when the
                                //installer is run and there is no database or connection string defined.
                                //the reason this method may get called during the installation is that the 
                                //SqlHelper of this class is shared amongst everything "Application" wide.

                                //TODO: Perhaps we should log something  here??
                                return null;
                            }
                        });
            }                   
        }

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
                    var connectionString = string.Empty;

                    try
                    {
                        const string umbracoDsn = Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName;
                    
                        var databaseSettings = ConfigurationManager.ConnectionStrings[umbracoDsn];
                        if (databaseSettings != null)
                            connectionString = databaseSettings.ConnectionString;

                        // During upgrades we might still have the old appSettings connectionstring, and not the new one, so get that one instead
                        if (string.IsNullOrWhiteSpace(connectionString) &&
                            ConfigurationManager.AppSettings.ContainsKey(umbracoDsn))
                            connectionString = ConfigurationManager.AppSettings[umbracoDsn];

                        _sqlHelper = DataLayerHelper.CreateSqlHelper(connectionString, false);
                    }
                    catch(Exception ex)
                    {
                        LogHelper.Error<Application>(string.Format("Can't instantiate SQLHelper with connectionstring \"{0}\"", connectionString), ex);
                    }
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
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the application alias.
        /// </summary>
        /// <value>The alias.</value>
        public string alias { get; set; }

        /// <summary>
        /// Gets or sets the application icon.
        /// </summary>
        /// <value>The application icon.</value>
        public string icon { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int sortOrder { get; set; }

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
            var exist = getAll().Any(x => x.alias == alias);
            
            if (!exist)
            {
                LoadXml(doc =>
                {
                    doc.Root.Add(new XElement("add",
                        new XAttribute("alias", alias),
                        new XAttribute("name", name),
                        new XAttribute("icon", icon),
                        new XAttribute("sortOrder", sortOrder)));
                }, true);

                //raise event
                OnNew(new Application(name, alias, icon, sortOrder), new EventArgs());
            }
        }

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

            LoadXml(doc =>
            {
                doc.Root.Elements("add").Where(x => x.Attribute("alias") != null && x.Attribute("alias").Value == this.alias).Remove();
            }, true);

            //raise event
            OnDeleted(this, new EventArgs());
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

                        //remove the cache so it gets re-read ... SD: I'm leaving this here even though it
                        // is taken care of by events as well, I think unit tests may rely on it being cleared here.
                        ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.ApplicationsCacheKey);
                    }
                }
            }
        }

        internal static event TypedEventHandler<Application, EventArgs> Deleted;
        private static void OnDeleted(Application app, EventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(app, args);
            }
        }

        internal static event TypedEventHandler<Application, EventArgs> New;
        private static void OnNew(Application app, EventArgs args)
        {
            if (New != null)
            {
                New(app, args);
            }
        }
    }
}
