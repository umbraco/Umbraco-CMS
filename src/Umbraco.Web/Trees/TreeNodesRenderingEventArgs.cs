using System.Net.Http.Formatting;

namespace Umbraco.Web.Trees
{
    public class TreeNodesRenderingEventArgs : TreeRenderingEventArgs
    {
        public TreeNodeCollection Nodes { get; private set; }

        public TreeNodesRenderingEventArgs(TreeNodeCollection nodes, FormDataCollection queryStrings)
            : base(queryStrings)
        {
            Nodes = nodes;
        }
    }
}