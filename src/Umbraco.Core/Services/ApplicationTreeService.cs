using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Cache;
using File = System.IO.File;

namespace Umbraco.Core.Services
{
    internal class ApplicationTreeService : IApplicationTreeService
    {
        private readonly ILogger _logger;
        private readonly CacheHelper _cache;
        private IEnumerable<ApplicationTree> _allAvailableTrees;
        private volatile bool _isInitialized = false;
        internal const string TreeConfigFileName = "trees.config";
        private static string _treeConfig;
        private static readonly object Locker = new object();

        public ApplicationTreeService(ILogger logger, CacheHelper cache)
        {
            _logger = logger;
            _cache = cache;
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
        private List<ApplicationTree> GetAppTrees()
        {
            return _cache.RuntimeCache.GetCacheItem<List<ApplicationTree>>(
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


                }, new TimeSpan(0, 10, 0));
        }

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
        public void Intitialize(IEnumerable<ApplicationTree> allAvailableTrees)
        {
            _allAvailableTrees = allAvailableTrees;
        }

        /// <summary>
        /// Creates a new application tree.
        /// </summary>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="title">The title.</param>
        /// <param name="iconClosed">The icon closed.</param>
        /// <param name="iconOpened">The icon opened.</param>
        /// <param name="type">The type.</param>
        public void MakeNew(bool initialize, byte sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type)
        {
            LoadXml(doc =>
            {
                var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == alias && x.Attribute("application").Value == applicationAlias);

                if (el == null)
                {
                    doc.Root.Add(new XElement("add",
                        new XAttribute("initialize", initialize),
                        new XAttribute("sortOrder", sortOrder),
                        new XAttribute("alias", alias),
                        new XAttribute("application", applicationAlias),
                        new XAttribute("title", title),
                        new XAttribute("iconClosed", iconClosed),
                        new XAttribute("iconOpen", iconOpened),
                        new XAttribute("type", type)));
                }

                return true;

            }, true);

            OnNew(new ApplicationTree(initialize, sortOrder, applicationAlias, alias, title, iconClosed, iconOpened, type), new EventArgs());
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void SaveTree(ApplicationTree tree)
        {
            LoadXml(doc =>
            {
                var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == tree.Alias && x.Attribute("application").Value == tree.ApplicationAlias);

                if (el != null)
                {
                    el.RemoveAttributes();

                    el.Add(new XAttribute("initialize", tree.Initialize));
                    el.Add(new XAttribute("sortOrder", tree.SortOrder));
                    el.Add(new XAttribute("alias", tree.Alias));
                    el.Add(new XAttribute("application", tree.ApplicationAlias));
                    el.Add(new XAttribute("title", tree.Title));
                    el.Add(new XAttribute("iconClosed", tree.IconClosed));
                    el.Add(new XAttribute("iconOpen", tree.IconOpened));
                    el.Add(new XAttribute("type", tree.Type));
                }

                return true;

            }, true);

            OnUpdated(tree, new EventArgs());
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void DeleteTree(ApplicationTree tree)
        {
            LoadXml(doc =>
            {
                doc.Root.Elements("add")
                    .Where(x => x.Attribute("application") != null
                                && x.Attribute("application").Value == tree.ApplicationAlias
                                && x.Attribute("alias") != null && x.Attribute("alias").Value == tree.Alias).Remove();

                return true;

            }, true);

            OnDeleted(tree, new EventArgs());
        }

        /// <summary>
        /// Gets an ApplicationTree by it's tree alias.
        /// </summary>
        /// <param name="treeAlias">The tree alias.</param>
        /// <returns>An ApplicationTree instance</returns>
        public ApplicationTree GetByAlias(string treeAlias)
        {
            return GetAppTrees().Find(t => (t.Alias == treeAlias));

        }

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        public IEnumerable<ApplicationTree> GetAll()
        {
            return GetAppTrees().OrderBy(x => x.SortOrder);
        }

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias)
        {
            return GetApplicationTrees(applicationAlias, false);
        }

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="onlyInitialized"></param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias, bool onlyInitialized)
        {
            var list = GetAppTrees().FindAll(
                t =>
                {
                    if (onlyInitialized)
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
        internal void LoadXml(Func<XDocument, bool> callback, bool saveAfterCallbackIfChanges)
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
                        _cache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
                    }
                }
            }
        }

        private List<ApplicationTree> ReadFromXmlAndSort()
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

                    var clrType = Type.GetType(type);
                    if (clrType == null)
                    {
                        _logger.Warn<ApplicationTreeService>("The tree definition: " + addElement.ToString() + " could not be resolved to a .Net object type");
                        continue;
                    }

                    //check if the tree definition (applicationAlias + type + assembly) is already in the list

                    if (list.Any(tree => tree.ApplicationAlias.InvariantEquals(applicationAlias) && tree.GetRuntimeType() == clrType) == false)
                    {
                        list.Add(new ApplicationTree(
                                     addElement.Attribute("initialize") == null || Convert.ToBoolean(addElement.Attribute("initialize").Value),
                                     addElement.Attribute("sortOrder") != null ? Convert.ToByte(addElement.Attribute("sortOrder").Value) : (byte)0,
                                     addElement.Attribute("application").Value,
                                     addElement.Attribute("alias").Value,
                                     addElement.Attribute("title").Value,
                                     addElement.Attribute("iconClosed").Value,
                                     addElement.Attribute("iconOpen").Value,
                                     addElement.Attribute("type").Value));
                    }
                }

                return false;

            }, false);

            return list;
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
