using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    /// <summary>
    /// A notification that allows developer to modify the root tree node that is being rendered
    /// </summary>
    public class RootNodeRendering : INotification
    {
        /// <summary>
        /// The root node being rendered
        /// </summary>
        public TreeNode Node { get; }

        /// <summary>
        /// The query string of the current request
        /// </summary>
        public FormCollection QueryString { get; }

        public RootNodeRendering(TreeNode node, FormCollection queryString)
        {
            Node = node;
            QueryString = queryString;
        }
    }
}
