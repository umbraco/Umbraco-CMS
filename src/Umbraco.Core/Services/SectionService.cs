using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using File = System.IO.File;

namespace Umbraco.Core.Services
{
    internal class SectionService : ISectionService
    {
        private readonly IUserService _userService;
        private readonly IApplicationTreeService _applicationTreeService;
        private readonly CacheHelper _cache;

        public SectionService(
            IUserService userService,
            IApplicationTreeService applicationTreeService, 
            CacheHelper cache)
        {
            if (applicationTreeService == null) throw new ArgumentNullException("applicationTreeService");
            if (cache == null) throw new ArgumentNullException("cache");

            _userService = userService;
            _applicationTreeService = applicationTreeService;
            _cache = cache;
        }

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
        /// Ensures all available sections exist in the storage medium
        /// </summary>
        /// <param name="existingSections"></param>
        public void Initialize(IEnumerable<Section> existingSections)
        {
            LoadXml(doc =>
            {
                foreach (var attr in existingSections)
                {
                    doc.Root.Add(new XElement("add",
                                              new XAttribute("alias", attr.Alias),
                                              new XAttribute("name", attr.Name),
                                              new XAttribute("icon", attr.Icon),
                                              new XAttribute("sortOrder", attr.SortOrder)));
                }
            }, true);
        }

        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        public IEnumerable<Section> GetSections()
        {
            return _cache.GetCacheItem(
                CacheKeys.ApplicationsCacheKey,
                () =>
                    {
                        ////used for unit tests
                        //if (_testApps != null)
                        //    return _testApps;

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
                                                        addElement.Attribute("icon").Value,
                                                        sortOrderAttr != null ? Convert.ToInt32(sortOrderAttr.Value) : 0));
                                }
                            }, false);
                        return tmp;
                    });
        }

        internal void LoadXml(Action<XDocument> callback, bool saveAfterCallback)
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
                        _cache.ClearCacheItem(CacheKeys.ApplicationsCacheKey);
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
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void MakeNew(string name, string alias, string icon)
        {
            MakeNew(name, alias, icon, GetSections().Max(x => x.SortOrder) + 1);
        }

        /// <summary>
        /// Makes the new.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="sortOrder">The sort order.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
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
                }, true);

                //raise event
                OnNew(new Section(name, alias, icon, sortOrder), new EventArgs());
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
                ApplicationContext.Current.DatabaseContext.Database.Execute(
                    "delete from umbracoUser2App where app = @appAlias",
                    new { appAlias = section.Alias });

                //delete the assigned trees
                var trees = _applicationTreeService.GetApplicationTrees(section.Alias);
                foreach (var t in trees)
                {
                    _applicationTreeService.DeleteTree(t);
                }

                LoadXml(doc =>
                {
                    doc.Root.Elements("add").Where(x => x.Attribute("alias") != null && x.Attribute("alias").Value == section.Alias).Remove();
                }, true);

                //raise event
                OnDeleted(section, new EventArgs());   
            }            
        }

        internal static event TypedEventHandler<Section, EventArgs> Deleted;
        private static void OnDeleted(Section app, EventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(app, args);
            }
        }

        internal static event TypedEventHandler<Section, EventArgs> New;
        private static void OnNew(Section app, EventArgs args)
        {
            if (New != null)
            {
                New(app, args);
            }
        }

    }
}
