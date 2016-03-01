using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
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

        internal const string TreeConfigFileName = "trees.config";
        private static string _treeConfig;
        private static readonly object Locker = new object();
        private static volatile bool _isInitialized = false;
        private static IEnumerable<ApplicationTree> _allAvailableTrees;

        /// <summary>
        /// Initializes the service with any trees found in plugins
        /// </summary>
        /// <param name="allAvailableTrees">
        /// A collection of all available tree found in assemblies in the application
        /// </param>
        /// <remarks>
        /// This will update the trees.config with the found tree plugins that are not currently listed in the file when the first
        /// access is made to resolve the tree collection
        /// </remarks>
        internal static void Intitialize(IEnumerable<ApplicationTree> allAvailableTrees)
        {
            _allAvailableTrees = allAvailableTrees;
        }

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
        /// The main entry point to get application trees
        /// </summary>
        /// <remarks>
        /// This lazily on first access will scan for plugin trees and ensure the trees.config is up-to-date with the plugins. If plugins
        /// haven't changed on disk then the file will not be saved. The trees are all then loaded from this config file into cache and returned.
        /// </remarks>
        private static List<ApplicationTree> GetAppTrees()
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem<List<ApplicationTree>>(
                CacheKeys.ApplicationTreeCacheKey,
                () =>
                {
                    var list = ReadFromXmlAndSort();

                    //On first access we need to do some initialization
                    if (_isInitialized == false)
                    {
                        lock (Locker)
                        {
                            if (_isInitialized == false)
                            {
                                //now we can check the non-volatile flag
                                if (_allAvailableTrees != null)
                                {
                                    var hasChanges = false;

                                    LoadXml(doc =>
                                    {
                                        //Now, load in the xml structure and update it with anything that is not declared there and save the file.

                                        //NOTE: On the first iteration here, it will lazily scan all trees, etc... this is because this ienumerable is lazy
                                        // based on the ApplicationTreeRegistrar - and as noted there this is not an ideal way to do things but were stuck like this
                                        // currently because of the legacy assemblies and types not in the Core.

                                        //Get all the trees not registered in the config
                                        var unregistered = _allAvailableTrees
                                            .Where(x => list.Any(l => l.Alias == x.Alias) == false)
                                            .ToArray();

                                        hasChanges = unregistered.Any();

                                        if (hasChanges == false) return false;

                                        //add the unregistered ones to the list and re-save the file if any changes were found
                                        var count = 0;
                                        foreach (var tree in unregistered)
                                        {
                                            doc.Root.Add(new XElement("add",
                                                new XAttribute("initialize", tree.Initialize),
                                                new XAttribute("sortOrder", tree.SortOrder),
                                                new XAttribute("alias", tree.Alias),
                                                new XAttribute("application", tree.ApplicationAlias),
                                                new XAttribute("title", tree.Title),
                                                new XAttribute("iconClosed", tree.IconClosed),
                                                new XAttribute("iconOpen", tree.IconOpened),
                                                new XAttribute("type", tree.Type)));
                                            count++;
                                        }

                                        //don't save if there's no changes
                                        return count > 0;
                                    }, true);

                                    if (hasChanges)
                                    {
                                        //If there were changes, we need to re-read the structures from the XML
                                        list = ReadFromXmlAndSort();
                                    }
                                }

                                _isInitialized = true;
                            }
                        }
                    }


                    return list;


                });
        }

        private static List<ApplicationTree> ReadFromXmlAndSort()
        {
            var list = new List<ApplicationTree>();

            //read in the xml file containing trees and convert them all to ApplicationTree instances
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

                    var clrType = System.Type.GetType(type);
                    if (clrType == null)
                    {
                        LogHelper.Warn(typeof(ApplicationTree), "The tree definition: " + addElement.ToString() + " could not be resolved to a .Net object type");
                        continue;
                    }

                    //check if the tree definition (applicationAlias + type + assembly) is already in the list

                    if (list.Any(tree => tree.ApplicationAlias.InvariantEquals(applicationAlias) && tree.GetRuntimeType() == clrType) == false)
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

                return false;

            }, false);

            return list;
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

        private Type _runtimeType;

        /// <summary>
        /// Returns the CLR type based on it's assembly name stored in the config
        /// </summary>
        /// <returns></returns>
        internal Type GetRuntimeType()
        {
            return _runtimeType ?? (_runtimeType = System.Type.GetType(Type));
        }

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

                return true;

            }, true);

            OnNew(new ApplicationTree(silent, initialize, sortOrder, applicationAlias, alias, title, iconClosed, iconOpened, assemblyName, type, action), new EventArgs());
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

                return true;

            }, true);

            OnUpdated(this, new EventArgs());
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

                return true;

            }, true);

            OnDeleted(this, new EventArgs());
        }


        /// <summary>
        /// Gets an ApplicationTree by it's tree alias.
        /// </summary>
        /// <param name="treeAlias">The tree alias.</param>
        /// <returns>An ApplicationTree instance</returns>
        public static ApplicationTree getByAlias(string treeAlias)
        {
            return GetAppTrees().Find(t => (t.Alias == treeAlias));

        }

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getAll()
        {
            return GetAppTrees().OrderBy(x => x.SortOrder).ToArray();
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
            var list = GetAppTrees().FindAll(
                t =>
                    {
                        if (onlyInitializedApplications)
                            return (t.ApplicationAlias == applicationAlias && t.Initialize);
                        return (t.ApplicationAlias == applicationAlias);
                    }
                );

            return list.OrderBy(x => x.SortOrder).ToArray();
        }

        /// <summary>
        /// Loads in the xml structure from disk if one is found, otherwise loads in an empty xml structure, calls the 
        /// callback with the xml document and saves the structure back to disk if saveAfterCallback is true.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="saveAfterCallbackIfChanges"></param>
        internal static void LoadXml(Func<XDocument, bool> callback, bool saveAfterCallbackIfChanges)
        {
            lock (Locker)
            {
                var doc = File.Exists(TreeConfigFilePath)
                    ? XDocument.Load(TreeConfigFilePath)
                    : XDocument.Parse("<?xml version=\"1.0\"?><trees />");

                if (doc.Root != null)
                {
                    var hasChanges = callback.Invoke(doc);

                    if (saveAfterCallbackIfChanges && hasChanges
                        //Don't save it if it is empty, in some very rare cases if the app domain get's killed in the middle of this process 
                        // in some insane way the file saved will be empty. I'm pretty sure it's not actually anything to do with the xml doc and
                        // more about the IO trying to save the XML doc, but it doesn't hurt to check.
                        && doc.Root != null && doc.Root.Elements().Any())
                    {
                        //ensures the folder exists
                        Directory.CreateDirectory(Path.GetDirectoryName(TreeConfigFilePath));

                        //saves it
                        doc.Save(TreeConfigFilePath);

                        //remove the cache now that it has changed  SD: I'm leaving this here even though it
                        // is taken care of by events as well, I think unit tests may rely on it being cleared here.
                        ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
                    }
                }
            }
        }

        internal static event TypedEventHandler<ApplicationTree, EventArgs> Deleted;
        private static void OnDeleted(ApplicationTree app, EventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(app, args);
            }
        }

        internal static event TypedEventHandler<ApplicationTree, EventArgs> New;
        private static void OnNew(ApplicationTree app, EventArgs args)
        {
            if (New != null)
            {
                New(app, args);
            }
        }

        internal static event TypedEventHandler<ApplicationTree, EventArgs> Updated;
        private static void OnUpdated(ApplicationTree app, EventArgs args)
        {
            if (Updated != null)
            {
                Updated(app, args);
            }
        }
    }
}
