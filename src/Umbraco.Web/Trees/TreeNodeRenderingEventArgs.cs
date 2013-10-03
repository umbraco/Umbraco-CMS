using System.Net.Http.Formatting;

namespace Umbraco.Web.Trees
{
    public class TreeNodeRenderingEventArgs : TreeRenderingEventArgs
    {
        public TreeNode Node { get; private set; }

        public TreeNodeRenderingEventArgs(TreeNode node, FormDataCollection queryStrings)
            : base(queryStrings)
        {
            Node = node;
        }
    }
}