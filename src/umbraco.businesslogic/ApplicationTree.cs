using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using umbraco.BusinessLogic.Utils;
using umbraco.DataLayer;
using umbraco.IO;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// umbraco.BusinessLogic.ApplicationTree provides access to the application tree structure in umbraco.
    /// An application tree is a collection of nodes belonging to one or more application(s).
    /// Through this class new application trees can be created, modified and deleted. 
    /// </summary>
    public class ApplicationTree
    {

        private const string CACHE_KEY = "ApplicationTreeCache";

        private static readonly string _appTreeConfig =
            IOHelper.MapPath(SystemDirectories.Config + "/trees.config");
        
        private static readonly object m_Locker = new object();

        /// <summary>
        /// The cache storage for all application trees
        /// </summary>
        private static List<ApplicationTree> AppTrees
        {
            get
            {
                //ensure cache exists
                if (HttpRuntime.Cache[CACHE_KEY] == null)
                    ReCache();
                return HttpRuntime.Cache[CACHE_KEY] as List<ApplicationTree>;
            }
            set
            {
                HttpRuntime.Cache.Insert(CACHE_KEY, value);
            }
        }


        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        private bool _silent;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> is silent.
        /// </summary>
        /// <value><c>true</c> if silent; otherwise, <c>false</c>.</value>
        public bool Silent
        {
            get { return _silent; }
            set { _silent = value; }
        }

        private bool _initialize;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> should initialize.
        /// </summary>
        /// <value><c>true</c> if initialize; otherwise, <c>false</c>.</value>
        public bool Initialize
        {
            get { return _initialize; }
            set { _initialize = value; }
        }

        private byte _sortOrder;
        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public byte SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        private string _applicationAlias;
        /// <summary>
        /// Gets the application alias.
        /// </summary>
        /// <value>The application alias.</value>
        public string ApplicationAlias
        {
            get { return _applicationAlias; }
        }

        private string _alias;
        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias
        {
            get { return _alias; }
        }

        private string _title;
        /// <summary>
        /// Gets or sets the tree title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _iconClosed;
        /// <summary>
        /// Gets or sets the icon closed.
        /// </summary>
        /// <value>The icon closed.</value>
        public string IconClosed
        {
            get { return _iconClosed; }
            set { _iconClosed = value; }
        }

        private string _iconOpened;
        /// <summary>
        /// Gets or sets the icon opened.
        /// </summary>
        /// <value>The icon opened.</value>
        public string IconOpened
        {
            get { return _iconOpened; }
            set { _iconOpened = value; }
        }

        private string _assemblyName;
        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        public string AssemblyName
        {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        private string _type;
        /// <summary>
        /// Gets or sets the tree type.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _action;
        /// <summary>
        /// Gets or sets the default tree action.
        /// </summary>
        /// <value>The action.</value>
        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }

        /// <summary>
        /// A static constructor that will cache all application trees
        /// </summary>
        static ApplicationTree()
        {
            Cache();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        /// </summary>
        public ApplicationTree() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationTree"/> class.
        /// </summary>
        /// <param name="silent">if set to <c>true</c> [silent].</param>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="alias">The tree alias.</param>
        /// <param name="title">The tree title.</param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpened">The icon opened.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="type">The tree type.</param>
        /// <param name="action">The default tree action.</param>
        public ApplicationTree(bool silent, bool initialize, byte sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string assemblyName, string type, string action)
        {
            this._silent = silent;
            this._initialize = initialize;
            this._sortOrder = sortOrder;
            this._applicationAlias = applicationAlias;
            this._alias = alias;
            this._title = title;
            this._iconClosed = iconClosed;
            this._iconOpened = iconOpened;
            this._assemblyName = assemblyName;
            this._type = type;
            this._action = action;
        }


        /// <summary>
        /// Creates a new application tree.
        /// </summary>
        /// <param name="silent">if set to <c>true</c> [silent].</param>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="title">The title.</param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpened">The icon opened.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="type">The type.</param>
        /// <param name="action">The action.</param>
        public static void MakeNew(bool silent, bool initialize, byte sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string assemblyName, string type, string action)
        {

//            SqlHelper.ExecuteNonQuery(@"insert into umbracoAppTree(treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, 
//                                        treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType, action) 
//                                        values(@treeSilent, @treeInitialize, @treeSortOrder, @appAlias, @treeAlias, @treeTitle, @treeIconClosed, @treeIconOpen, @treeHandlerAssembly, @treeHandlerType, @action)"
//                                        ,
//                SqlHelper.CreateParameter("@treeSilent", silent),
//                SqlHelper.CreateParameter("@treeInitialize", initialize),
//                SqlHelper.CreateParameter("@treeSortOrder", sortOrder),
//                SqlHelper.CreateParameter("@treeAlias", alias),
//                SqlHelper.CreateParameter("@appAlias", applicationAlias),
//                SqlHelper.CreateParameter("@treeTitle", title),
//                SqlHelper.CreateParameter("@treeIconClosed", iconClosed),
//                SqlHelper.CreateParameter("@treeIconOpen", iconOpened),
//                SqlHelper.CreateParameter("@treeHandlerAssembly", assemblyName),
//                SqlHelper.CreateParameter("@treeHandlerType", type),
//                SqlHelper.CreateParameter("@action", action)
//                );

            LoadXml(doc =>
            {
                doc.Root.Add(new XElement("add",
                    new XAttribute("silent", silent),
                    new XAttribute("initialize", initialize),
                    new XAttribute("sortOrder", sortOrder),
                    new XAttribute("alias", alias),
                    new XAttribute("application", applicationAlias),
                    new XAttribute("title", title),
                    new XAttribute("iconClosed", iconClosed),
                    new XAttribute("iconOpen", iconOpened),
                    new XAttribute("assembly", assemblyName),
                    new XAttribute("type", type),
                    new XAttribute("action", string.IsNullOrEmpty(action) ? "" : action)));
            }, true);
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
//            SqlHelper.ExecuteNonQuery(@"Update umbracoAppTree set treeSilent = @treeSilent, treeInitialize = @treeInitialize, treeSortOrder = @treeSortOrder, treeTitle = @treeTitle, 
//                                        treeIconClosed = @treeIconClosed, treeIconOpen = @treeIconOpen, treeHandlerAssembly = @treeHandlerAssembly, treeHandlerType = @treeHandlerType, action = @action 
//                                        where treeAlias = @treeAlias AND appAlias = @appAlias",
//                SqlHelper.CreateParameter("@treeSilent", this.Silent),
//                SqlHelper.CreateParameter("@treeInitialize", this.Initialize),
//                SqlHelper.CreateParameter("@treeSortOrder", this.SortOrder),
//                SqlHelper.CreateParameter("@treeTitle", this.Title),
//                SqlHelper.CreateParameter("@treeIconClosed", this.IconClosed),
//                SqlHelper.CreateParameter("@treeIconOpen", this.IconOpened),
//                SqlHelper.CreateParameter("@treeHandlerAssembly", this.AssemblyName),
//                SqlHelper.CreateParameter("@treeHandlerType", this.Type),
//                SqlHelper.CreateParameter("@treeAlias", this.Alias),
//                SqlHelper.CreateParameter("@appAlias", this.ApplicationAlias),
//                SqlHelper.CreateParameter("@action", this.Action)
//                );

            LoadXml(doc =>
            {
                var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == this.Alias && x.Attribute("application").Value == this.ApplicationAlias);

                if(el != null)
                {
                    el.RemoveAttributes();

                    el.Add(new XAttribute("silent", this.Silent));
                    el.Add(new XAttribute("initialize", this.Initialize));
                    el.Add(new XAttribute("sortOrder", this.SortOrder));
                    el.Add(new XAttribute("alias", this.Alias));
                    el.Add(new XAttribute("application", this.ApplicationAlias));
                    el.Add(new XAttribute("title", this.Title));
                    el.Add(new XAttribute("iconClosed", this.IconClosed));
                    el.Add(new XAttribute("iconOpen", this.IconOpened));
                    el.Add(new XAttribute("assembly", this.AssemblyName));
                    el.Add(new XAttribute("type", this.Type));
                    el.Add(new XAttribute("action", string.IsNullOrEmpty(this.Action) ? "" : this.Action));
                }

            }, true);

        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            //SqlHelper.ExecuteNonQuery("delete from umbracoAppTree where appAlias = @appAlias AND treeAlias = @treeAlias",
            //    SqlHelper.CreateParameter("@appAlias", this.ApplicationAlias), SqlHelper.CreateParameter("@treeAlias", this.Alias));

            LoadXml(doc =>
            {
                doc.Root.Elements("add").Where(x => x.Attribute("application") != null && x.Attribute("application").Value == this.ApplicationAlias && 
                x.Attribute("alias") != null && x.Attribute("alias").Value == this.Alias).Remove();
            }, true);
        }


        /// <summary>
        /// Gets an ApplicationTree by it's tree alias.
        /// </summary>
        /// <param name="treeAlias">The tree alias.</param>
        /// <returns>An ApplicationTree instance</returns>
        public static ApplicationTree getByAlias(string treeAlias)
        {
            return AppTrees.Find(
                delegate(ApplicationTree t)
                {
                    return (t.Alias == treeAlias);
                }
            );

        }

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getAll()
        {
            return AppTrees.OrderBy(x => x.SortOrder).ToArray();
        }

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getApplicationTree(string applicationAlias)
        {
            return getApplicationTree(applicationAlias, false);
        }

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="onlyInitializedApplications"></param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getApplicationTree(string applicationAlias, bool onlyInitializedApplications)
        {
            List<ApplicationTree> list = AppTrees.FindAll(
                delegate(ApplicationTree t)
                {
                    if (onlyInitializedApplications)
                        return (t.ApplicationAlias == applicationAlias && t.Initialize);
                    else
                        return (t.ApplicationAlias == applicationAlias);
                }
            );

            return list.OrderBy(x => x.SortOrder).ToArray();
        }

        /// <summary>
        /// Removes the ApplicationTree cache and re-reads the data from the db.
        /// </summary>
        private static void ReCache()
        {
            HttpRuntime.Cache.Remove(CACHE_KEY);
            Cache();
        }

        /// <summary>
        /// Read all ApplicationTree data and store it in cache.
        /// </summary>
        private static void Cache()
        {
            //don't query the database if the cache is not null
            if (HttpRuntime.Cache[CACHE_KEY] == null)
            {
                lock (m_Locker)
                {
                    if (HttpRuntime.Cache[CACHE_KEY] == null)
                    {
                        List<ApplicationTree> list = new List<ApplicationTree>();

//                        using (IRecordsReader dr = SqlHelper.ExecuteReader(@"Select treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, 
//                                                                treeIconOpen, treeHandlerAssembly, treeHandlerType, action from umbracoAppTree order by treeSortOrder"))
//                        {
//                            while (dr.Read())
//                            {

//                                list.Add(new ApplicationTree(
//                                    dr.GetBoolean("treeSilent"),
//                                    dr.GetBoolean("treeInitialize"),
//                                    dr.GetByte("treeSortOrder"),
//                                    dr.GetString("appAlias"),
//                                    dr.GetString("treeAlias"),
//                                    dr.GetString("treeTitle"),
//                                    dr.GetString("treeIconClosed"),
//                                    dr.GetString("treeIconOpen"),
//                                    dr.GetString("treeHandlerAssembly"),
//                                    dr.GetString("treeHandlerType"),
//                                    dr.GetString("action")));

//                            }
//                        }

                        LoadXml(doc =>
                        {
                            foreach (var addElement in doc.Root.Elements("add").OrderBy(x =>
                            {
                                var sortOrderAttr = x.Attribute("sortOrder");
                                return sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0;
                            }))
                            {
                                list.Add(new ApplicationTree(
                                    addElement.Attribute("silent") != null ? Convert.ToBoolean(addElement.Attribute("silent").Value) : false,
                                    addElement.Attribute("initialize") != null ? Convert.ToBoolean(addElement.Attribute("initialize").Value) : true,
                                    addElement.Attribute("sortOrder") != null ? Convert.ToByte(addElement.Attribute("sortOrder").Value) : (byte)0,
                                    addElement.Attribute("application").Value,
                                    addElement.Attribute("alias").Value,
                                    addElement.Attribute("title").Value,
                                    addElement.Attribute("iconClosed").Value,
                                    addElement.Attribute("iconOpen").Value,
                                    addElement.Attribute("assembly").Value,
                                    addElement.Attribute("type").Value,
                                    addElement.Attribute("action") != null ? addElement.Attribute("action").Value : ""));
                            }
                        }, false);

                        AppTrees = list;
                    }
                }
            }
            
        }

        internal static void LoadXml(Action<XDocument> callback, bool saveAfterCallback)
        {
            lock (m_Locker)
            {
                var doc = File.Exists(_appTreeConfig)
                    ? XDocument.Load(_appTreeConfig)
                    : XDocument.Parse("<?xml version=\"1.0\"?><trees />");
                if (doc.Root != null)
                {
                    callback.Invoke(doc);

                    if (saveAfterCallback)
                    {
                        doc.Save(_appTreeConfig);

                        ReCache();
                    }
                }
            }
        }
    }

    public class ApplicationTreeRegistrar : ApplicationStartupHandler
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

        public ApplicationTreeRegistrar()
        {
            // Load all Applications by attribute and add them to the XML config
            var types = TypeFinder.FindClassesOfType<ITree>()
                .Where(x => x.GetCustomAttributes(typeof(ApplicationTreeAttribute), false).Any());

            var items = types.Select(x => new Tuple<Type, ApplicationTreeAttribute>(x,
                (ApplicationTreeAttribute)x.GetCustomAttributes(typeof(ApplicationTreeAttribute), false).Single()))
                .Where(x => ApplicationTree.getByAlias(x.Item2.Alias) == null);

            var allAliases = ApplicationTree.getAll().Select(x => x.Alias).Concat(items.Select(x => x.Item2.Alias));
            var inString = "'" + string.Join("','", allAliases) + "'";

            ApplicationTree.LoadXml(doc =>
            {
                foreach (var tuple in items)
                {
                    var type = tuple.Item1;
                    var attr = tuple.Item2;

                    var typeParts = type.AssemblyQualifiedName.Split(',');
                    var assemblyName = typeParts[1].Trim();
                    var typeName = typeParts[0].Substring(assemblyName.Length + 1).Trim();

                    doc.Root.Add(new XElement("add",
                        new XAttribute("silent", attr.Silent),
                        new XAttribute("initialize", attr.Initialize),
                        new XAttribute("sortOrder", attr.SortOrder),
                        new XAttribute("alias", attr.Alias),
                        new XAttribute("application", attr.ApplicationAlias),
                        new XAttribute("title", attr.Title),
                        new XAttribute("iconClosed", attr.IconClosed),
                        new XAttribute("iconOpen", attr.IconOpen),
                        new XAttribute("assembly", assemblyName),
                        new XAttribute("type", typeName),
                        new XAttribute("action", attr.Action)));
                }

                var dbTrees = SqlHelper.ExecuteReader("SELECT * FROM umbracoAppTree WHERE treeAlias NOT IN (" + inString + ")");
                while(dbTrees.Read())
                {
                    var action = dbTrees.GetString("action");

                    doc.Root.Add(new XElement("add",
                        new XAttribute("silent", dbTrees.GetBoolean("treeSilent")),
                        new XAttribute("initialize", dbTrees.GetBoolean("treeInitialize")),
                        new XAttribute("sortOrder", dbTrees.GetByte("treeSortOrder")),
                        new XAttribute("alias", dbTrees.GetString("treeAlias")),
                        new XAttribute("application", dbTrees.GetString("appAlias")),
                        new XAttribute("title", dbTrees.GetString("treeTitle")),
                        new XAttribute("iconClosed", dbTrees.GetString("treeIconClosed")),
                        new XAttribute("iconOpen", dbTrees.GetString("treeIconOpen")),
                        new XAttribute("assembly", dbTrees.GetString("treeHandlerAssembly")),
                        new XAttribute("type", dbTrees.GetString("treeHandlerType")),
                        new XAttribute("action", string.IsNullOrEmpty(action) ? "" : action)));
                }

            }, true);

            //SqlHelper.ExecuteNonQuery("DELETE FROM umbracoAppTree");
        }
    }
}
