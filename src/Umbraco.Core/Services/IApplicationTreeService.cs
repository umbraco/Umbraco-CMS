using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IApplicationTreeService
    {
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
        void Intitialize(IEnumerable<ApplicationTree> allAvailableTrees);

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
        void MakeNew(bool initialize, byte sortOrder, string applicationAlias, string alias, string title, string iconClosed, string iconOpened, string type);

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
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias);

        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="applicationAlias">The application alias.</param>
        /// <param name="onlyInitialized"></param>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias, bool onlyInitialized);
    }
}