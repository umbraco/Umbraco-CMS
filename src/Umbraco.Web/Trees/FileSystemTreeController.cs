using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web._Legacy.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract string FilePath { get; }
        protected abstract IEnumerable<string> FileSearchPattern { get; }
        protected abstract string EditFormUrl { get; }
        protected abstract bool EnableCreateOnFolder { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        /// <param name="xNode"></param>
        protected virtual void OnRenderFileNode(TreeNode treeNode, FileInfo file)
        {
        }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        /// <param name="xNode"></param>
        protected virtual void OnRenderFolderNode(TreeNode treeNode)
        {
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            string orgPath = "";
            string path = "";
            if (!string.IsNullOrEmpty(id) && id != Constants.System.Root.ToInvariantString())
            {
                orgPath = id;
                path = IOHelper.MapPath(FilePath + "/" + orgPath);
                orgPath += "/";
            }
            else
            {
                path = IOHelper.MapPath(FilePath);
            }

            if (!Directory.Exists(path) && !System.IO.File.Exists(path))
            {
                return nodes;
            }

            if (System.IO.File.Exists(path))
            {
                return GetTreeNodesForFile(path, id, queryStrings);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo[] dirInfos = new DirectoryInfo(path).GetDirectories();

            foreach (DirectoryInfo dir in dirInfos)
            {
                if ((dir.Attributes.HasFlag(FileAttributes.Hidden)) == false)
                {
                    var hasChildren = dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0;
                    var node = CreateTreeNode(orgPath + dir.Name, orgPath, queryStrings, dir.Name, "icon-folder",
                        hasChildren);

                    //TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
                    node.AdditionalData["jsClickCallback"] = "javascript:void(0);";
                    OnRenderFolderNode(node);

                    nodes.Add(node);
                }
            }

            var files = FileSearchPattern
                .SelectMany(p => dirInfo.GetFiles("*." + p))
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));

            foreach (FileInfo file in files)
            {
                var nodeId = orgPath + file.Name;

                var node = CreateTreeNode(
                    nodeId,
                    orgPath, queryStrings,
                    file.Name.StripFileExtension(),
                    "icon-file",
                    false,
                    "/" + queryStrings.GetValue<string>("application") + "/framed/" +
                    Uri.EscapeDataString(string.Format(EditFormUrl, nodeId)));

                OnRenderFileNode(node, file);

                nodes.Add(node);
            }

            return nodes;
        }

        protected virtual TreeNodeCollection GetTreeNodesForFile(string path, string id, FormDataCollection queryStrings)
        {
            return new TreeNodeCollection();
        }
        

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {

            var menu = new MenuItemCollection();
            
            OnBeforeRenderMenu(menu, id, queryStrings);

            if (id == Constants.System.Root.ToInvariantString())
            {
                //Create the normal create action
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    //Since we haven't implemented anything for file systems in angular, this needs to be converted to 
                    //use the legacy format
                    .ConvertLegacyFileSystemMenuItem("", "init" + TreeAlias, queryStrings.GetValue<string>("application"));

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);

                return menu;

            }
            
            if (Directory.Exists(IOHelper.MapPath(FilePath + "/" + id)))
            {
                if (EnableCreateOnFolder)
                {
                    //Create the normal create action
                    menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                        //Since we haven't implemented anything for file systems in angular, this needs to be converted to 
                        //use the legacy format
                        .ConvertLegacyFileSystemMenuItem(id, TreeAlias + "Folder",
                            queryStrings.GetValue<string>("application"));
                }

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);

            }

            //add delete option for all languages
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias), true)
                .ConvertLegacyFileSystemMenuItem(
                    id, TreeAlias, queryStrings.GetValue<string>("application"));

            OnAfterRenderMenu(menu, id, queryStrings);

            return menu;
        }

        protected virtual void OnBeforeRenderMenu(MenuItemCollection menu, string id, FormDataCollection queryStrings)
        {
        }

        protected virtual void OnAfterRenderMenu(MenuItemCollection menu, string id, FormDataCollection queryStrings)
        {
            
        }
    }
}
