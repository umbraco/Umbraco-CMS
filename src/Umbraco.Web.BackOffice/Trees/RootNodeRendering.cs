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
        public TreeNode Node { get; }

        public FormCollection QueryStrings { get; }

        public RootNodeRendering(TreeNode node, FormCollection queryStrings)
        {
            Node = node;
            QueryStrings = queryStrings;
        }
    }
}
