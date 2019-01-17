using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Services
{
    internal class ApplicationTreeService : IApplicationTreeService
    {
        private readonly ILogger _logger;
        private readonly TreeCollection _treeCollection;
        private static readonly object Locker = new object();
        private readonly Lazy<IReadOnlyCollection<IGrouping<string, string>>> _groupedTrees;

        public ApplicationTreeService(ILogger logger, TreeCollection treeCollection)
        {
            _logger = logger;
            _treeCollection = treeCollection;
            //_groupedTrees = new Lazy<IReadOnlyCollection<IGrouping<string, string>>>(InitGroupedTrees);
        }

        ///// <summary>
        ///// gets/sets the trees.config file path
        ///// </summary>
        ///// <remarks>
        ///// The setter is generally only going to be used in unit tests, otherwise it will attempt to resolve it using the IOHelper.MapPath
        ///// </remarks>
        //internal static string TreeConfigFilePath
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(_treeConfig))
        //        {
        //            _treeConfig = IOHelper.MapPath(SystemDirectories.Config + "/" + TreeConfigFileName);
        //        }
        //        return _treeConfig;
        //    }
        //    set { _treeConfig = value; }
        //}

        ///// <summary>
        ///// The main entry point to get application trees
        ///// </summary>
        ///// <remarks>
        ///// This lazily on first access will scan for plugin trees and ensure the trees.config is up-to-date with the plugins. If plugins
        ///// haven't changed on disk then the file will not be saved. The trees are all then loaded from this config file into cache and returned.
        ///// </remarks>
        //private List<ApplicationTree> GetAppTrees()
        //{
        //    return _cache.RuntimeCache.GetCacheItem<List<ApplicationTree>>(
        //        CacheKeys.ApplicationTreeCacheKey,
        //        () =>
        //        {
        //            var list = ReadFromXmlAndSort();

        //            //now we can check the non-volatile flag
        //            if (_allAvailableTrees != null)
        //            {
        //                var hasChanges = false;

        //                LoadXml(doc =>
        //                {
        //                    //Now, load in the xml structure and update it with anything that is not declared there and save the file.

        //                    //NOTE: On the first iteration here, it will lazily scan all trees, etc... this is because this ienumerable is lazy
        //                    // based on the ApplicationTreeRegistrar - and as noted there this is not an ideal way to do things but were stuck like this
        //                    // currently because of the legacy assemblies and types not in the Core.

        //                                //Get all the trees not registered in the config (those not matching by alias casing will be detected as "unregistered")
        //                    var unregistered = _allAvailableTrees.Value
        //                        .Where(x => list.Any(l => l.Alias == x.Alias) == false)
        //                        .ToArray();

        //                    hasChanges = unregistered.Any();

        //                    if (hasChanges == false) return false;

        //                                //add or edit the unregistered ones and re-save the file if any changes were found
        //                    var count = 0;
        //                    foreach (var tree in unregistered)
        //                    {
        //                                    var existingElement = doc.Root.Elements("add").SingleOrDefault(x =>
        //                                        string.Equals(x.Attribute("alias").Value, tree.Alias,
        //                                            StringComparison.InvariantCultureIgnoreCase) &&
        //                                        string.Equals(x.Attribute("application").Value, tree.ApplicationAlias,
        //                                            StringComparison.InvariantCultureIgnoreCase));
        //                                    if (existingElement != null)
        //                                    {
        //                                        existingElement.SetAttributeValue("alias", tree.Alias);
        //                                    }
        //                                    else
        //                                    {
        //                                        if (tree.Title.IsNullOrWhiteSpace())
        //                                        {
        //                                            doc.Root.Add(new XElement("add",
        //                                                new XAttribute("initialize", tree.Initialize),
        //                                                new XAttribute("sortOrder", tree.SortOrder),
        //                                                new XAttribute("alias", tree.Alias),
        //                                                new XAttribute("application", tree.ApplicationAlias),
        //                                                new XAttribute("iconClosed", tree.IconClosed),
        //                                                new XAttribute("iconOpen", tree.IconOpened),
        //                                                new XAttribute("type", tree.Type)));
        //                                        }
        //                                        else
        //                                        {
        //                        doc.Root.Add(new XElement("add",
        //                            new XAttribute("initialize", tree.Initialize),
        //                            new XAttribute("sortOrder", tree.SortOrder),
        //                            new XAttribute("alias", tree.Alias),
        //                            new XAttribute("application", tree.ApplicationAlias),
        //                            new XAttribute("title", tree.Title),
        //                            new XAttribute("iconClosed", tree.IconClosed),
        //                            new XAttribute("iconOpen", tree.IconOpened),
        //                            new XAttribute("type", tree.Type)));
        //                                        }

        //                                    }
        //                        count++;
        //                    }

        //                    //don't save if there's no changes
        //                    return count > 0;
        //                }, true);

        //                if (hasChanges)
        //                {
        //                    //If there were changes, we need to re-read the structures from the XML
        //                    list = ReadFromXmlAndSort();
        //                }
        //            }

        //            return list;
        //        }, new TimeSpan(0, 10, 0));
        //}

        ///// <summary>
        ///// Creates a new application tree.
        ///// </summary>
        ///// <param name="initialize">if set to <c>true</c> [initialize].</param>
        ///// <param name="sortOrder">The sort order.</param>
        ///// <param name="applicationAlias">The application alias.</param>
        ///// <param name="alias">The alias.</param>
        ///// <param name="title">The title.</param>
        ///// <param name="iconClosed">The icon closed.</param>
        ///// <param name="iconOpened">The icon opened.</param>
        ///// <param name="type">The type.</param>
        //public void MakeNew(bool initialize, int sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type)
        //{
        //    LoadXml(doc =>
        //    {
        //        var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == alias && x.Attribute("application").Value == applicationAlias);

        //        if (el == null)
        //        {
        //            doc.Root.Add(new XElement("add",
        //                new XAttribute("initialize", initialize),
        //                new XAttribute("sortOrder", sortOrder),
        //                new XAttribute("alias", alias),
        //                new XAttribute("application", applicationAlias),
        //                new XAttribute("title", title),
        //                new XAttribute("iconClosed", iconClosed),
        //                new XAttribute("iconOpen", iconOpened),
        //                new XAttribute("type", type)));
        //        }

        //        return true;

        //    }, true);

        //    OnNew(new ApplicationTree(initialize, sortOrder, applicationAlias, alias, title, iconClosed, iconOpened, type), new EventArgs());
        //}

        ///// <summary>
        ///// Saves this instance.
        ///// </summary>
        //public void SaveTree(ApplicationTree tree)
        //{
        //    LoadXml(doc =>
        //    {
        //        var el = doc.Root.Elements("add").SingleOrDefault(x => x.Attribute("alias").Value == tree.Alias && x.Attribute("application").Value == tree.ApplicationAlias);

        //        if (el != null)
        //        {
        //            el.RemoveAttributes();

        //            el.Add(new XAttribute("initialize", tree.Initialize));
        //            el.Add(new XAttribute("sortOrder", tree.SortOrder));
        //            el.Add(new XAttribute("alias", tree.Alias));
        //            el.Add(new XAttribute("application", tree.ApplicationAlias));
        //            el.Add(new XAttribute("title", tree.Title));
        //            el.Add(new XAttribute("iconClosed", tree.IconClosed));
        //            el.Add(new XAttribute("iconOpen", tree.IconOpened));
        //            el.Add(new XAttribute("type", tree.Type));
        //        }

        //        return true;

        //    }, true);

        //    OnUpdated(tree, new EventArgs());
        //}

        ///// <summary>
        ///// Deletes this instance.
        ///// </summary>
        //public void DeleteTree(ApplicationTree tree)
        //{
        //    LoadXml(doc =>
        //    {
        //        doc.Root.Elements("add")
        //            .Where(x => x.Attribute("application") != null
        //                        && x.Attribute("application").Value == tree.ApplicationAlias
        //                        && x.Attribute("alias") != null && x.Attribute("alias").Value == tree.Alias).Remove();

        //        return true;

        //    }, true);

        //    OnDeleted(tree, new EventArgs());
        //}

        /// <inheritdoc />
        public ApplicationTree GetByAlias(string treeAlias) => _treeCollection.FirstOrDefault(t => t.Alias == treeAlias);

        /// <inheritdoc />
        public IEnumerable<ApplicationTree> GetAll() => _treeCollection;

        /// <inheritdoc />
        public IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias)
            => GetAll().Where(x => x.ApplicationAlias.InvariantEquals(applicationAlias)).OrderBy(x => x.SortOrder).ToList();

        ///// <summary>
        ///// Gets the application tree for the applcation with the specified alias
        ///// </summary>
        ///// <param name="applicationAlias">The application alias.</param>
        ///// <param name="onlyInitialized"></param>
        ///// <returns>Returns a ApplicationTree Array</returns>
        //public IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias, bool onlyInitialized)
        //{
        //    var list = GetAppTrees().FindAll(
        //        t =>
        //        {
        //            if (onlyInitialized)
        //                return (t.ApplicationAlias == applicationAlias && t.Initialize);
        //            return (t.ApplicationAlias == applicationAlias);
        //        }
        //        );

        //    return list.OrderBy(x => x.SortOrder).ToArray();
        //}

        public IDictionary<string, IEnumerable<ApplicationTree>> GetGroupedApplicationTrees(string applicationAlias)
        {
            var result = new Dictionary<string, IEnumerable<ApplicationTree>>();
            var foundTrees = GetApplicationTrees(applicationAlias).ToList();
            foreach(var treeGroup in _groupedTrees.Value)
            {
                List<ApplicationTree> resultGroup = null;
                foreach(var tree in foundTrees)
                { 
                    foreach(var treeAliasInGroup in treeGroup)
                    {
                        if (tree.Alias == treeAliasInGroup)
                        {
                            if (resultGroup == null) resultGroup = new List<ApplicationTree>();
                            resultGroup.Add(tree);
                        }
                    }  
                }
                if (resultGroup != null)
                    result[treeGroup.Key ?? string.Empty] = resultGroup; //key cannot be null so make empty string
            }
            return result;
        }

        ///// <summary>
        ///// Creates a group of all tree groups and their tree aliases
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>
        ///// Used to initialize the <see cref="_groupedTrees"/> field
        ///// </remarks>
        //private IReadOnlyCollection<IGrouping<string, string>> InitGroupedTrees()
        //{
        //    var result = GetAll()
        //        .Select(x => (treeAlias: x.Alias, treeGroup: x.GetRuntimeType().GetCustomAttribute<CoreTreeAttribute>(false)?.TreeGroup))
        //        .GroupBy(x => x.treeGroup, x => x.treeAlias)
        //        .ToList();
        //    return result;
        //}

        ///// <summary>
        ///// Loads in the xml structure from disk if one is found, otherwise loads in an empty xml structure, calls the
        ///// callback with the xml document and saves the structure back to disk if saveAfterCallback is true.
        ///// </summary>
        ///// <param name="callback"></param>
        ///// <param name="saveAfterCallbackIfChanges"></param>
        //internal void LoadXml(Func<XDocument, bool> callback, bool saveAfterCallbackIfChanges)
        //{
        //    lock (Locker)
        //    {
        //        var doc = System.IO.File.Exists(TreeConfigFilePath)
        //            ? XDocument.Load(TreeConfigFilePath)
        //            : XDocument.Parse("<?xml version=\"1.0\"?><trees />");

        //        if (doc.Root != null)
        //        {
        //            var hasChanges = callback.Invoke(doc);

        //            if (saveAfterCallbackIfChanges && hasChanges
        //                //Don't save it if it is empty, in some very rare cases if the app domain get's killed in the middle of this process
        //                // in some insane way the file saved will be empty. I'm pretty sure it's not actually anything to do with the xml doc and
        //                // more about the IO trying to save the XML doc, but it doesn't hurt to check.
        //                && doc.Root != null && doc.Root.Elements().Any())
        //            {
        //                //ensures the folder exists
        //                Directory.CreateDirectory(Path.GetDirectoryName(TreeConfigFilePath));

        //                //saves it
        //                doc.Save(TreeConfigFilePath);

        //                //remove the cache now that it has changed  SD: I'm leaving this here even though it
        //                // is taken care of by events as well, I think unit tests may rely on it being cleared here.
        //                _cache.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
        //            }
        //        }
        //    }
        //}

        //private List<ApplicationTree> ReadFromXmlAndSort()
        //{
        //    var list = new List<ApplicationTree>();

        //    //read in the xml file containing trees and convert them all to ApplicationTree instances
        //    LoadXml(doc =>
        //    {
        //        foreach (var addElement in doc.Root.Elements("add").OrderBy(x =>
        //        {
        //            var sortOrderAttr = x.Attribute("sortOrder");
        //            return sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0;
        //        }))
        //        {
        //            var applicationAlias = (string)addElement.Attribute("application");
        //            var type = (string)addElement.Attribute("type");
        //            var assembly = (string)addElement.Attribute("assembly");

        //            var clrType = Type.GetType(type);
        //            if (clrType == null)
        //            {
        //                _logger.Warn<ApplicationTreeService>("The tree definition: {AddElement} could not be resolved to a .Net object type", addElement);
        //                continue;
        //            }

        //            //check if the tree definition (applicationAlias + type + assembly) is already in the list

        //            if (list.Any(tree => tree.ApplicationAlias.InvariantEquals(applicationAlias) && tree.GetRuntimeType() == clrType) == false)
        //            {
        //                list.Add(new ApplicationTree(
        //                             addElement.Attribute("initialize") == null || Convert.ToBoolean(addElement.Attribute("initialize").Value),
        //                             addElement.Attribute("sortOrder") != null
        //                                ? Convert.ToByte(addElement.Attribute("sortOrder").Value)
        //                                : (byte)0,
        //                             (string)addElement.Attribute("application"),
        //                             (string)addElement.Attribute("alias"),
        //                             (string)addElement.Attribute("title"),
        //                             (string)addElement.Attribute("iconClosed"),
        //                             (string)addElement.Attribute("iconOpen"),
        //                             (string)addElement.Attribute("type")));
        //            }
        //        }

        //        return false;

        //    }, false);

        //    return list;
        //}


        //internal static event TypedEventHandler<ApplicationTree, EventArgs> Deleted;
        //private static void OnDeleted(ApplicationTree app, EventArgs args)
        //{
        //    if (Deleted != null)
        //    {
        //        Deleted(app, args);
        //    }
        //}

        //internal static event TypedEventHandler<ApplicationTree, EventArgs> New;
        //private static void OnNew(ApplicationTree app, EventArgs args)
        //{
        //    if (New != null)
        //    {
        //        New(app, args);
        //    }
        //}

        //internal static event TypedEventHandler<ApplicationTree, EventArgs> Updated;
        //private static void OnUpdated(ApplicationTree app, EventArgs args)
        //{
        //    if (Updated != null)
        //    {
        //        Updated(app, args);
        //    }
        //}

        ///// <summary>
        ///// This class is here so that we can provide lazy access to tree scanning for when it is needed
        ///// </summary>
        //private class LazyEnumerableTrees : IEnumerable<ApplicationTree>
        //{
        //    public LazyEnumerableTrees(TypeLoader typeLoader)
        //    {
        //        _lazyTrees = new Lazy<IEnumerable<ApplicationTree>>(() =>
        //        {
        //            var added = new List<string>();

        //            // Load all Controller Trees by attribute
        //            var types = typeLoader.GetTypesWithAttribute<TreeController, TreeAttribute>(); // fixme inject
        //            //convert them to ApplicationTree instances
        //            var items = types
        //                .Select(x => (tree: x, treeAttribute: x.GetCustomAttributes<TreeAttribute>(false).Single()))
        //                .Select(x => new ApplicationTree(x.treeAttribute.Initialize, x.treeAttribute.SortOrder, x.treeAttribute.ApplicationAlias, x.treeAttribute.Alias, x.treeAttribute.Title, x.treeAttribute.IconClosed, x.treeAttribute.IconOpen, x.tree.GetFullNameWithAssembly()))
        //                .ToArray();

        //            added.AddRange(items.Select(x => x.Alias));

        //            return items.ToArray();
        //        });
        //    }

        //    private readonly Lazy<IEnumerable<ApplicationTree>> _lazyTrees;

        //    /// <summary>
        //    /// Returns an enumerator that iterates through the collection.
        //    /// </summary>
        //    /// <returns>
        //    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        //    /// </returns>
        //    public IEnumerator<ApplicationTree> GetEnumerator()
        //    {
        //        return _lazyTrees.Value.GetEnumerator();
        //    }

        //    /// <summary>
        //    /// Returns an enumerator that iterates through a collection.
        //    /// </summary>
        //    /// <returns>
        //    /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        //    /// </returns>
        //    IEnumerator IEnumerable.GetEnumerator()
        //    {
        //        return GetEnumerator();
        //    }
        //}
    }
}
