using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Services
{
    public interface ITreeService
    {
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

        ///// <summary>
        ///// Gets the application tree for the applcation with the specified alias
        ///// </summary>
        ///// <param name="applicationAlias">The application alias.</param>
        ///// <param name="onlyInitialized"></param>
        ///// <returns>Returns a ApplicationTree Array</returns>
        //IEnumerable<ApplicationTree> GetApplicationTrees(string applicationAlias, bool onlyInitialized);

        /// <summary>
        /// Gets the grouped application trees for the application with the specified alias
        /// </summary>
        /// <param name="applicationAlias"></param>
        /// <returns></returns>
        IDictionary<string, IEnumerable<ApplicationTree>> GetGroupedApplicationTrees(string applicationAlias);
    }
    
}
