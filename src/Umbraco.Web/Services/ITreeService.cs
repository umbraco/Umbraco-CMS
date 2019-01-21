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
        Tree GetByAlias(string treeAlias);

        /// <summary>
        /// Gets all applicationTrees registered in umbraco from the umbracoAppTree table..
        /// </summary>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<Tree> GetAll();
        
        /// <summary>
        /// Gets the application tree for the applcation with the specified alias
        /// </summary>
        /// <param name="sectionAlias">The application alias.</param>
        /// <returns>Returns a ApplicationTree Array</returns>
        IEnumerable<Tree> GetTrees(string sectionAlias);
        
        /// <summary>
        /// Gets the grouped application trees for the application with the specified alias
        /// </summary>
        /// <param name="sectionAlias"></param>
        /// <returns></returns>
        IDictionary<string, IEnumerable<Tree>> GetGroupedTrees(string sectionAlias);
    }
    
}
