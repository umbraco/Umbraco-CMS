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
        public TreeNodeCollection Nodes { get; }

        public FormCollection QueryStrings { get; }

        public TreeNodesRendering(TreeNodeCollection nodes, FormCollection queryStrings)
        {
            Nodes = nodes;
            QueryStrings = queryStrings;
        }
    }
}
