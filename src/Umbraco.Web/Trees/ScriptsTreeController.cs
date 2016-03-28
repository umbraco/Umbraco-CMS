using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, Constants.Trees.Scripts, "Scripts", "icon-folder", "icon-folder", sortOrder: 2)]
    public class ScriptsTreeController : FileSystemTreeController
    {
        protected override string FilePath
        {
            get { return SystemDirectories.Scripts + "/"; }
        }

        protected override IEnumerable<string> FileSearchPattern
        {
            get { return UmbracoConfig.For.UmbracoSettings().Content.ScriptFileTypes; }
        }

        protected override string EditFormUrl
        {
            get { return "settings/scripts/editScript.aspx?file={0}"; }
        }

        protected override bool EnableCreateOnFolder
        {
            get { return true; }
        }

        protected override void OnRenderFolderNode(TreeNode treeNode)
        {
        }

        protected override void OnRenderFileNode(TreeNode treeNode, FileInfo file)
        {
            treeNode.Icon =
                file.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ?
                "icon-script" : 
                "icon-code";

        }
    }
}
