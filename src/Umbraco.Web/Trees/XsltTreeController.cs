using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, Constants.Trees.Xslt, "XSLT Files", "icon-folder", "icon-folder", sortOrder: 2)]
    public class XsltTreeController : FileSystemTreeController
    {
        protected override string FilePath
        {
            get { return SystemDirectories.Xslt + "/"; }
        }

        protected override IEnumerable<string> FileSearchPattern
        {
            get { return new [] {"xslt"}; }
        }

        protected override string EditFormUrl
        {
            get { return "developer/xslt/editXslt.aspx?file={0}"; }
        }

        protected override bool EnableCreateOnFolder
        {
            get { return false; }
        }

        protected override void OnRenderFileNode(TreeNode treeNode, FileInfo file)
        {
            treeNode.Icon = "icon-code";
        }
    }
}
