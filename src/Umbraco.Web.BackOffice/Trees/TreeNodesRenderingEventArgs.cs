using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Trees;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    public class TreeNodesRenderingEventArgs : TreeRenderingEventArgs
    {
        public TreeNodeCollection Nodes { get; private set; }

        public TreeNodesRenderingEventArgs(TreeNodeCollection nodes, FormCollection queryStrings)
            : base(queryStrings)
        {
            Nodes = nodes;
        }
    }
}
