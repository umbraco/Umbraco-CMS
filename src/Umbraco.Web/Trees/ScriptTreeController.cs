using umbraco;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Scripts)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Scripts, null, sortOrder: 4)]
    [LegacyBaseTree(typeof(loadScripts))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
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
