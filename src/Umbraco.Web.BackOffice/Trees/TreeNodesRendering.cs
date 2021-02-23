using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    /// <summary>
    /// A notification that allows developers to modify the tree node collection that is being rendered
    /// </summary>
    /// <remarks>
    /// Developers can add/remove/replace/insert/update/etc... any of the tree items in the collection.
    /// </remarks>
    public class TreeNodesRendering : INotification
    {
        /// <summary>
        /// The tree nodes being rendered
        /// </summary>
        public TreeNodeCollection Nodes { get; }

        /// <summary>
        /// The query string of the current request
        /// </summary>
        public FormCollection QueryString { get; }

        public TreeNodesRendering(TreeNodeCollection nodes, FormCollection queryString)
        {
            Nodes = nodes;
            QueryString = queryString;
        }
    }
}
