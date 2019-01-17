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
using Umbraco.Core.Models;
using Umbraco.Core.Composing;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using File = System.IO.File;

namespace Umbraco.Web.Services
{
    internal class SectionService : ISectionService
    {
        private readonly IUserService _userService;
        private readonly Lazy<IEnumerable<Section>> _allAvailableSections;
        private readonly IApplicationTreeService _applicationTreeService;
        private readonly IScopeProvider _scopeProvider;
        private readonly AppCaches _cache;
        internal const string AppConfigFileName = "applications.config";
        private static string _appConfig;
        private static readonly object Locker = new object();

        public SectionService(
            IUserService userService,
            IApplicationTreeService applicationTreeService,
            IScopeProvider scopeProvider,
            AppCaches cache)
        {
            _applicationTreeService = applicationTreeService ?? throw new ArgumentNullException(nameof(applicationTreeService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _userService = userService;
            _scopeProvider = scopeProvider;
            _allAvailableSections = new Lazy<IEnumerable<Section>>(() => new LazyEnumerableSections());
        }


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
            set => _appConfig = value;
        }

        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        public IEnumerable<Section> GetSections()
        {
            return _cache.RuntimeCache.GetCacheItem<IEnumerable<Section>>(
                CacheKeys.ApplicationsCacheKey,
                () =>
                    {
                        var list = ReadFromXmlAndSort();
                        var hasChanges = false;
                        var localCopyList = list;

                        LoadXml(doc =>
                        {
                            //Now, load in the xml structure and update it with anything that is not declared there and save the file.
                            //NOTE: On the first iteration here, it will lazily scan all apps, etc... this is because this ienumerable is lazy
                            //Get all the trees not registered in the config

                            var unregistered = _allAvailableSections.Value
                                .Where(x => localCopyList.Any(l => l.Alias == x.Alias) == false)
                                .ToArray();

                            hasChanges = unregistered.Any();

                            var count = 0;
                            foreach (var attr in unregistered)
                            {
                                doc.Root.Add(new XElement("add",
                                    new XAttribute("alias", attr.Alias),
                                    new XAttribute("name", attr.Name),
                                    new XAttribute("sortOrder", attr.SortOrder)));
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

                        return list;

                    }, new TimeSpan(0, 10, 0));
        }

        internal void LoadXml(Func<XDocument, bool> callback, bool saveAfterCallbackIfChanged)
        {
            lock (Locker)
            {
                var doc = File.Exists(AppConfigFilePath)
                    ? XDocument.Load(AppConfigFilePath)
                    : XDocument.Parse("<?xml version=\"1.0\"?><applications />");

                if (doc.Root != null)
                {
                    var changed = callback.Invoke(doc);

                    if (saveAfterCallbackIfChanged && changed)
                    {
                        //ensure the folder is created!
                        Directory.CreateDirectory(Path.GetDirectoryName(AppConfigFilePath));

                        doc.Save(AppConfigFilePath);

                        //remove the cache so it gets re-read ... SD: I'm leaving this here even though it
                        // is taken care of by events as well, I think unit tests may rely on it being cleared here.
                        _cache.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationsCacheKey);
                    }
                }
            }
        }

        /// <summary>
        /// Get the user's allowed sections
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<Section> GetAllowedSections(int userId)
        {

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                throw new InvalidOperationException("No user found with id " + userId);
            }

            return GetSections().Where(x => user.AllowedSections.Contains(x.Alias));
        }

        /// <summary>
        /// Gets the application by its alias.
        /// </summary>
        /// <param name="appAlias">The application alias.</param>
        /// <returns></returns>
        public Section GetByAlias(string appAlias)
        {
            return GetSections().FirstOrDefault(t => t.Alias == appAlias);
        }

        /// <summary>
        /// Creates a new applcation if no application with the specified alias is found.
        /// </summary>
        /// <param name="name">The application name.</param>
        /// <param name="alias">The application alias.</param>
        /// <param name="icon">The application icon, which has to be located in umbraco/images/tray folder.</param>
        public void MakeNew(string name, string alias, string icon)
        {
            var sections = GetSections();
            var nextSortOrder = sections.Any() ? sections.Max(x => x.SortOrder) + 1 : 1;
            MakeNew(name, alias, icon, nextSortOrder);
        }

        /// <summary>
        /// Makes the new.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="sortOrder">The sort order.</param>
        public void MakeNew(string name, string alias, string icon, int sortOrder)
        {
            if (GetSections().All(x => x.Alias != alias))
            {
                LoadXml(doc =>
                {
                    doc.Root.Add(new XElement("add",
                        new XAttribute("alias", alias),
                        new XAttribute("name", name),
                        new XAttribute("icon", icon),
                        new XAttribute("sortOrder", sortOrder)));
                    return true;
                }, true);

                //raise event
                OnNew(this /*new Section(name, alias, sortOrder)*/, new EventArgs());
            }
        }

        /// <summary>
        /// Deletes the section
        /// </summary>
        public void DeleteSection(Section section)
        {
            lock (Locker)
            {
                //delete the assigned applications
                using (var scope = _scopeProvider.CreateScope())
                {
                    scope.Database.Execute("delete from umbracoUserGroup2App where app = @appAlias",
                        new { appAlias = section.Alias });
                    scope.Complete();
                }

                //delete the assigned trees
                var trees = _applicationTreeService.GetApplicationTrees(section.Alias);
                foreach (var t in trees)
                {
                    _applicationTreeService.DeleteTree(t);
                }

                LoadXml(doc =>
                {
                    doc.Root.Elements("add").Where(x => x.Attribute("alias") != null && x.Attribute("alias").Value == section.Alias)
                        .Remove();

                    return true;
                }, true);

                //raise event
                OnDeleted(this, new EventArgs());
            }
        }

        private List<Section> ReadFromXmlAndSort()
        {
            var tmp = new List<Section>();

            LoadXml(doc =>
            {
                foreach (var addElement in doc.Root.Elements("add").OrderBy(x =>
                {
                    var sortOrderAttr = x.Attribute("sortOrder");
                    return sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0;
                }))
                {
                    var sortOrderAttr = addElement.Attribute("sortOrder");
                    tmp.Add(new Section(addElement.Attribute("name").Value,
                                        addElement.Attribute("alias").Value,
                                        sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0));
                }
                return false;
            }, false);

            return tmp;
        }

        internal static event TypedEventHandler<ISectionService, EventArgs> Deleted;
        private static void OnDeleted(ISectionService app, EventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(app, args);
            }
        }

        internal static event TypedEventHandler<ISectionService, EventArgs> New;
        private static void OnNew(ISectionService app, EventArgs args)
        {
            if (New != null)
            {
                New(app, args);
            }
        }

        /// <summary>
        /// This class is here so that we can provide lazy access to tree scanning for when it is needed
        /// </summary>
        private class LazyEnumerableSections : IEnumerable<Section>
        {
            public LazyEnumerableSections()
            {
                _lazySections = new Lazy<IEnumerable<Section>>(() =>
                {
                    // Load all Applications by attribute and add them to the XML config

                    //don't cache the result of this because it is only used once during app startup, caching will just add a bit more mem overhead for no reason
                    var types = Current.TypeLoader.GetTypesWithAttribute<IApplication, ApplicationAttribute>(cache: false); // fixme - inject

                    //since applications don't populate their metadata from the attribute and because it is an interface,
                    //we need to interrogate the attributes for the data. Would be better to have a base class that contains
                    //metadata populated by the attribute. Oh well i guess.
                    var attrs = types.Select(x => x.GetCustomAttributes<ApplicationAttribute>(false).Single());
                    return attrs.Select(x => new Section(x.Name, x.Alias, x.SortOrder)).ToArray();
                });
            }

            private readonly Lazy<IEnumerable<Section>> _lazySections;

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<Section> GetEnumerator()
            {
                return _lazySections.Value.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }
}
