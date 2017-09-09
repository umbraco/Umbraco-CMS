using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, "scripts", null, sortOrder: 4)]
    public class ScriptTreeController : FileSystemTreeController
    {
        protected override IFileSystem2 FileSystem
        {
            get { return FileSystemProviderManager.Current.ScriptsFileSystem; }
        }

        private static readonly string[] ExtensionsStatic = { "js" };

        protected override string[] Extensions
        {
            get { return ExtensionsStatic; }
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
    }
}
