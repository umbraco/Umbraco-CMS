using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using umbraco.DataLayer;
using System.Runtime.CompilerServices;
using umbraco.businesslogic;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Class for handling all registered applications in Umbraco.
    /// </summary>
    [Obsolete("Use ApplicationContext.Current.Services.SectionService and/or Umbraco.Core.Sections.SectionCollection instead")]
    public class Application
    {
        private static ISqlHelper _sqlHelper;
        
        

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Do not use SqlHelper anymore, if database querying needs to be done use the DatabaseContext instead")]
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
        public static void MakeNew(string name, string alias, string icon)
        {
            ApplicationContext.Current.Services.SectionService.MakeNew(name, alias, icon);
        }

        /// <summary>
        /// Makes the new.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="sortOrder">The sort order.</param>
        public static void MakeNew(string name, string alias, string icon, int sortOrder)
        {
            ApplicationContext.Current.Services.SectionService.MakeNew(name, alias, icon, sortOrder);
        }

        /// <summary>
        /// Gets the application by its alias.
        /// </summary>
        /// <param name="appAlias">The application alias.</param>
        /// <returns></returns>
        public static Application getByAlias(string appAlias)
        {
            return Mapper.Map<Section, Application>(
                ApplicationContext.Current.Services.SectionService.GetByAlias(appAlias));
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            ApplicationContext.Current.Services.SectionService.DeleteSection(Mapper.Map<Application, Section>(this));
        }

        /// <summary>
        /// Gets all applications registered in umbraco from the umbracoApp table..
        /// </summary>
        /// <returns>Returns a Application Array</returns>
        public static List<Application> getAll()
        {
            return ApplicationContext.Current.Services.SectionService.GetSections().Select(Mapper.Map<Section, Application>).ToList();
        }

        /// <summary>
        /// Stores all references to classes that are of type IApplication
        /// </summary>
        [Obsolete("RegisterIApplications has been depricated. Please use ApplicationStartupHandler.RegisterHandlers() instead.")]
        public static void RegisterIApplications()
        {
            ApplicationStartupHandler.RegisterHandlers();
        }
   
        
    }
}
