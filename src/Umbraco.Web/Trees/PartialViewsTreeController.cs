using System.Collections.Generic;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Tree for displaying partial views in the settings app
    /// </summary>
    [Tree(Constants.Applications.Settings, Constants.Trees.PartialViews, "Partial Views", sortOrder: 2)]
    public class PartialViewsTreeController : FileSystemTreeController
    {
        protected override string FilePath
        {
            get { return SystemDirectories.MvcViews + "/Partials/"; }
        }

        protected override IEnumerable<string> FileSearchPattern
        {
            get { return new[] {"cshtml"}; }
        }

        protected override string EditFormUrl
        {
            get { return "Settings/Views/EditView.aspx?treeType=partialViews&file={0}"; }
        }

        protected override bool EnableCreateOnFolder
        {
            get { return true; }
        }

        protected override void OnRenderFileNode(TreeNode treeNode, FileInfo file)
        {
            treeNode.Icon = "icon-article";
        }
    }
}
