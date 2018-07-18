using umbraco;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Stylesheets)]
    [Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, null, sortOrder: 3)]
    [LegacyBaseTree(typeof(loadStylesheets))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class StylesheetTreeController : FileSystemTreeController
    {
        protected override IFileSystem2 FileSystem
        {
            get { return FileSystemProviderManager.Current.StylesheetsFileSystem; }
        }

        private static readonly string[] ExtensionsStatic = { "css" };

        protected override string[] Extensions
        {
            get { return ExtensionsStatic; }
        }
        protected override string FileIcon
        {
            get { return "icon-brackets"; }
        }

        protected override void OnRenderFolderNode(ref TreeNode treeNode)
        {
            //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }
    }
}
