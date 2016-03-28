using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

using umbraco.cms.businesslogic.template;
using umbraco.cms.presentation.Trees;
using Umbraco.Web.Models.Trees;
using Umbraco.Web._Legacy.Actions;

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
