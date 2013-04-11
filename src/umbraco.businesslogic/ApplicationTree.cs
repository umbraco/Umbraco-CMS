using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.DataLayer;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// umbraco.BusinessLogic.ApplicationTree provides access to the application tree structure in umbraco.
    /// An application tree is a collection of nodes belonging to one or more application(s).
    /// Through this class new application trees can be created, modified and deleted. 
    /// </summary>
    public class ApplicationTree
    {

        private const string CacheKey = "ApplicationTreeCache";
        internal const string TreeConfigFileName = "trees.config";
        private static string _treeConfig;
        private static readonly object Locker = new object();

        /// <summary>
        /// gets/sets the trees.config file path
        /// </summary>
        /// <remarks>
        /// The setter is generally only going to be used in unit tests, otherwise it will attempt to resolve it using the IOHelper.MapPath
        /// </remarks>
        internal static string TreeConfigFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_treeConfig))
                {
                    _treeConfig = IOHelper.MapPath(SystemDirectories.Config + "/" + TreeConfigFileName);
                }
                return _treeConfig;
            }
            set { _treeConfig = value; }
        }

        /// <summary>
        /// The cache storage for all application trees
        /// </summary>
        private static List<ApplicationTree> AppTrees
        {
            get
            {
                //ensure cache exists
                EnsureCache();
                return HttpRuntime.Cache[CacheKey] as List<ApplicationTree>;
            }
            set
            {
                HttpRuntime.Cache.Insert(CacheKey, value);
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

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> is silent.
        /// </summary>
        /// <value><c>true</c> if silent; otherwise, <c>false</c>.</value>
        public bool Silent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationTree"/> should initialize.
        /// </summary>
        /// <value><c>true</c> if initialize; otherwise, <c>false</c>.</value>
        public bool Initialize { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public byte SortOrder { get; set; }

        /// <summary>
        /// Gets the application alias.
        /// </summary>
        /// <value>The application alias.</value>
        public string ApplicationAlias { get; private set; }

        /// <summary>
        /// Gets the tree alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or sets the tree title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the icon closed.
        /// </summary>
        /// <value>The icon closed.</value>
        public string IconClosed { get; set; }

        /// <summary>
        /// Gets or sets the icon opened.
        /// </summary>
        /// <value>The icon opened.</value>
        public string IconOpened { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>The name of the assembly.</value>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the tree type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the default tree action.
        /// </summary>
        /// <value>The action.</value>
        public string Action { get; set; }

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
            this.Silent = silent;
            this.Initialize = initialize;
            this.SortOrder = sortOrder;
            this.ApplicationAlias = applicationAlias;
            this.Alias = alias;
            this.Title = title;
            this.IconClosed = iconClosed;
            this.IconOpened = iconOpened;
            this.AssemblyName = assemblyName;
            this.Type = type;
            this.Action = action;
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
            LoadXml(doc =>
            {
                var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == alias && x.Attribute("application").Value == applicationAlias);

                if (el == null)
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
                }
            }, true);
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            LoadXml(doc =>
            {
                var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == this.Alias && x.Attribute("application").Value == this.ApplicationAlias);

                if (el != null)
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
            HttpRuntime.Cache.Remove(CacheKey);
            EnsureCache();
        }

        /// <summary>
        /// Read all ApplicationTree data and store it in cache.
        /// </summary>
        private static void EnsureCache()
        {
            //don't query the database if the cache is not null
            if (HttpRuntime.Cache[CacheKey] != null) 
                return;
            
            lock (Locker)
            {
                if (HttpRuntime.Cache[CacheKey] == null)
                {
                    var list = new List<ApplicationTree>();

                    LoadXml(doc =>
                    {
                        foreach (var addElement in doc.Root.Elements("add").OrderBy(x =>
                                {
                                    var sortOrderAttr = x.Attribute("sortOrder");
                                    return sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0;
                                }))
                        {

                            var applicationAlias = (string)addElement.Attribute("application");
                            var type = (string)addElement.Attribute("type");
                            var assembly = (string)addElement.Attribute("assembly");

                            //check if the tree definition (applicationAlias + type + assembly) is already in the list

                            if (!list.Any(tree => tree.ApplicationAlias.InvariantEquals(applicationAlias)
                                && tree.Type.InvariantEquals(type)
                                && tree.AssemblyName.InvariantEquals(assembly)))
                            {
                                list.Add(new ApplicationTree(
                                             addElement.Attribute("silent") != null && Convert.ToBoolean(addElement.Attribute("silent").Value),
                                             addElement.Attribute("initialize") == null || Convert.ToBoolean(addElement.Attribute("initialize").Value),
                                             addElement.Attribute("sortOrder") != null ? Convert.ToByte(addElement.Attribute("sortOrder").Value) : (byte)0,
                                             addElement.Attribute("application").Value,
                                             addElement.Attribute("alias").Value,
                                             addElement.Attribute("title").Value,
                                             addElement.Attribute("iconClosed").Value,
                                             addElement.Attribute("iconOpen").Value,
                                             (string)addElement.Attribute("assembly"), //this could be empty: http://issues.umbraco.org/issue/U4-1360
                                             addElement.Attribute("type").Value,
                                             addElement.Attribute("action") != null ? addElement.Attribute("action").Value : ""));
                            }

                            
                        }
                    }, false);

                    AppTrees = list;
                }
            }
        }

        internal static void LoadXml(Action<XDocument> callback, bool saveAfterCallback)
        {
            lock (Locker)
            {
                var doc = File.Exists(TreeConfigFilePath)
                    ? XDocument.Load(TreeConfigFilePath)
                    : XDocument.Parse("<?xml version=\"1.0\"?><trees />");
                if (doc.Root != null)
                {
                    callback.Invoke(doc);

                    if (saveAfterCallback)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(TreeConfigFilePath));

                        doc.Save(TreeConfigFilePath);

                        ReCache();
                    }
                }
            }
        }
    }
}
