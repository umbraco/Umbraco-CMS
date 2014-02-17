using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using umbraco.DataLayer;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// umbraco.BusinessLogic.ApplicationTree provides access to the application tree structure in umbraco.
    /// An application tree is a collection of nodes belonging to one or more application(s).
    /// Through this class new application trees can be created, modified and deleted. 
    /// </summary>
    [Obsolete("This has been superceded by ApplicationContext.Current.ApplicationTreeService")]
    public class ApplicationTree
    {

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
            ApplicationContext.Current.Services.ApplicationTreeService.MakeNew(initialize, sortOrder, applicationAlias, alias, title, iconClosed, iconOpened,
                assemblyName.IsNullOrWhiteSpace() ? type : string.Format("{0}.{1},{0}", assemblyName, type));
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            ApplicationContext.Current.Services.ApplicationTreeService.SaveTree(
                Mapper.Map<ApplicationTree, Umbraco.Core.Models.ApplicationTree>(this));
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            ApplicationContext.Current.Services.ApplicationTreeService.DeleteTree(
                Mapper.Map<ApplicationTree, Umbraco.Core.Models.ApplicationTree>(this));
        }

        /// <summary>
        /// Gets an ApplicationTree by it's tree alias.
        /// </summary>
        /// <param name="treeAlias">The tree alias.</param>
        /// <returns>An ApplicationTree instance</returns>
        public static ApplicationTree getByAlias(string treeAlias)
        {
            return Mapper.Map<Umbraco.Core.Models.ApplicationTree, ApplicationTree>(
                ApplicationContext.Current.Services.ApplicationTreeService.GetByAlias(treeAlias));
        }

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getAll()
        {
            return ApplicationContext.Current.Services.ApplicationTreeService.GetAll()
                          .Select(Mapper.Map<Umbraco.Core.Models.ApplicationTree, ApplicationTree>)
                          .ToArray();
        }

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getApplicationTree(string applicationAlias)
        {
            return ApplicationContext.Current.Services.ApplicationTreeService.GetApplicationTrees(applicationAlias)
                          .Select(Mapper.Map<Umbraco.Core.Models.ApplicationTree, ApplicationTree>)
                          .ToArray();
        }

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="onlyInitializedApplications"></param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public static ApplicationTree[] getApplicationTree(string applicationAlias, bool onlyInitializedApplications)
        {
            return ApplicationContext.Current.Services.ApplicationTreeService.GetApplicationTrees(applicationAlias, onlyInitializedApplications)
                          .Select(Mapper.Map<Umbraco.Core.Models.ApplicationTree, ApplicationTree>)
                          .ToArray();
        }

    }
}
