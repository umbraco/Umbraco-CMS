using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Trees;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.BackOffice.Trees
{
    public class TreeNodeRenderingEventArgs : TreeRenderingEventArgs
    {
        public TreeNode Node { get; private set; }

        public TreeNodeRenderingEventArgs(TreeNode node, FormCollection queryStrings)
            : base(queryStrings)
        {
            Node = node;
        }
    }
}
