using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, "scripts", "Scripts", sortOrder: 4)]
    public class ScriptTreeController : FileSystemTreeController
    {
        protected override string FilePath
        {
            get { return SystemDirectories.Scripts; }
        }

        protected override string FileSearchPattern
        {
            get { return "*.js"; }
        }
        protected override string FileIcon
        {
            get { return "icon-script"; }
        }

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }

        protected override void OnRenderFileNode(ref TreeNode treeNode)
        {
            base.OnRenderFileNode(ref treeNode);
        }
    }
}
