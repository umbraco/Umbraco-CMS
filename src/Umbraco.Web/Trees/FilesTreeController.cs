using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    // this is not a section tree - do not mark with [Tree]
    [CoreTree]
    public class FilesTreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => new PhysicalFileSystem("~/");

        private static readonly string[] ExtensionsStatic = { "*" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-document";

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }
    }
}
