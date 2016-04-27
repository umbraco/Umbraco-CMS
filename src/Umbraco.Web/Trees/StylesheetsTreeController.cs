using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web._Legacy.Actions;
using File = System.IO.File;

namespace Umbraco.Web.Trees
{
    [Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, "Stylesheets", "icon-folder", "icon-folder", sortOrder: 3)]
    public class StylesheetsTreeController : FileSystemTreeController
    {
        
        protected override string FilePath
        {
            get { return SystemDirectories.Css + "/"; }
        }

        protected override IEnumerable<string> FileSearchPattern
        {
            get { return new [] {"css"}; }
        }

        protected override string EditFormUrl
        {
            get { return "settings/stylesheet/editStylesheet.aspx?id={0}"; }
        }

        protected override bool EnableCreateOnFolder
        {
            get { return false; }
        }
        

        protected override void OnBeforeRenderMenu(MenuItemCollection menu, string id, FormDataCollection queryStrings)
        {
            if (File.Exists((IOHelper.MapPath(FilePath + "/" + id))))
            {
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    //Since we haven't implemented anything for file systems in angular, this needs to be converted to 
                    //use the legacy format
                    .ConvertLegacyFileSystemMenuItem(id, "stylesheet", queryStrings.GetValue<string>("application"));
            }
        }

        protected override void OnAfterRenderMenu(MenuItemCollection menu, string id, FormDataCollection queryStrings)
        {
            if (File.Exists((IOHelper.MapPath(FilePath + "/" + id))))
            {
                menu.Items.Add<ActionSort>(Services.TextService.Localize("actions", ActionSort.Instance.Alias), true)
                    .ConvertLegacyFileSystemMenuItem(id, "stylesheet", queryStrings.GetValue<string>("application"));

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
            }
        }

        protected override void OnRenderFileNode(TreeNode treeNode, FileInfo file)
        {
            treeNode.Icon = "icon-brackets";
            treeNode.NodeType = "stylesheet";
            var styleSheet = Services.FileService.GetStylesheetByName(treeNode.Id.ToString().EnsureEndsWith(".css"));
            if (styleSheet != null)
            {
                treeNode.HasChildren = styleSheet.Properties.Any();
            }
            
        }

        protected override TreeNodeCollection GetTreeNodesForFile(string path, string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var sheet = Services.FileService.GetStylesheetByName(id.EnsureEndsWith(".css"));

            foreach (var prop in sheet.Properties)
            {
                var node = CreateTreeNode(
                    id + "_" + prop.Name,
                    id, queryStrings,
                   prop.Name,
                    "icon-brackets",
                    false,
                    "/" + queryStrings.GetValue<string>("application") + "/framed/" +
                    Uri.EscapeDataString("settings/stylesheet/property/editStylesheetProperty.aspx?id=" +
                    sheet.Path + "&prop=" + prop.Name));
                node.NodeType = "stylesheetProperty";
                nodes.Add(node);



            }

            return nodes;
        }
    }
}
