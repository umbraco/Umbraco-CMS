using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IApplicationTreeService
    {
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
        void MakeNew(bool initialize, int sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type);

        /// <summary>
        /// Saves this instance.
        /// </summary>
        void SaveTree(ApplicationTree tree);

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        void DeleteTree(ApplicationTree tree);

        /// <summary>
        /// Gets an ApplicationTree by it's tree alias.
        /// </summary>
        /// <param name="treeAlias">The tree alias.</param>
        /// <returns>An ApplicationTree instance</returns>
        ApplicationTree GetByAlias(string treeAlias);

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<ApplicationTree> GetAll();
        
        /// <summary>
        /// Gets the application tree for the application with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias);

        /// <summary>
        /// Gets the application tree for the application with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="onlyInitialized"></param>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias, bool onlyInitialized);

        /// <summary>
        /// Gets the grouped application trees for the application with the specified alias
        /// </summary>
        /// <param name="applicationAlias"></param>
        /// <param name="onlyInitialized"></param>
        /// <returns></returns>
        IDictionary<string, IEnumerable<ApplicationTree>> GetGroupedApplicationTrees(string applicationAlias, bool onlyInitialized);
    }

    /// <summary>
    /// Purely used to allow a service context to create the default services
    /// </summary>
    internal class EmptyApplicationTreeService : IApplicationTreeService
    {
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
        public void MakeNew(bool initialize, int sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void SaveTree(ApplicationTree tree)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void DeleteTree(ApplicationTree tree)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets an ApplicationTree by it's tree alias.
        /// </summary>
        /// <param name="treeAlias">The tree alias.</param>
        /// <returns>An ApplicationTree instance</returns>
        public ApplicationTree GetByAlias(string treeAlias)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        public IEnumerable<ApplicationTree> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public IDictionary<string, IEnumerable<ApplicationTree>> GetGroupedApplicationTrees(string applicationAlias, bool onlyInitialized)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the application tree for the application with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the application tree for the application with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="onlyInitialized"></param>
        /// <returns>Returns a ApplicationTree Array</returns>
        public IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias, bool onlyInitialized)
        {
            throw new System.NotImplementedException();
        }
    }
}
