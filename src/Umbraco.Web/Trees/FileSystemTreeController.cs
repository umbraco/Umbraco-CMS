using System;
using System.IO;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract string FilePath { get; }
        protected abstract string FileSearchPattern { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnRenderFileNode(ref TreeNode treeNode) { }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnRenderFolderNode(ref TreeNode treeNode) { }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var orgPath = "";
            string path;
            if (string.IsNullOrEmpty(id) == false && id != "-1")
            {
                orgPath = id;
                path = IOHelper.MapPath(FilePath + "/" + orgPath);
                orgPath += "/";
            }
            else
            {
                path = IOHelper.MapPath(FilePath);
            }

            var dirInfo = new DirectoryInfo(path);
            var dirInfos = dirInfo.GetDirectories();

            var nodes = new TreeNodeCollection();
            foreach (var dir in dirInfos)
            {
                if ((dir.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                var hasChildren = dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0;
                var node = CreateTreeNode(orgPath + dir.Name, orgPath, queryStrings, dir.Name, "icon-folder", hasChildren);

                OnRenderFolderNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            var allowedExtensions = new string[0];
            var filterByMultipleExtensions = FileSearchPattern.Contains(",");
            FileInfo[] fileInfo;

            if (filterByMultipleExtensions)
            {
                fileInfo = dirInfo.GetFiles();
                allowedExtensions = FileSearchPattern.ToLower().Split(',');
            }
            else
                fileInfo = dirInfo.GetFiles(FileSearchPattern);

            foreach (var file in fileInfo)
            {
                if ((file.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                if (filterByMultipleExtensions && Array.IndexOf(allowedExtensions, file.Extension.ToLower().Trim('.')) < 0)
                    continue;

                var withoutExt = Path.GetFileNameWithoutExtension(file.Name);
                if (withoutExt.IsNullOrWhiteSpace())
                    continue;

                var node = CreateTreeNode(orgPath + file.Name, orgPath, queryStrings, file.Name, "icon-file", false);

                OnRenderFileNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == Constants.System.Root.ToInvariantString())
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;
                //create action
                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);

                return menu;
            }

            string path;
            if (string.IsNullOrEmpty(id) == false)
            {
                var orgPath = System.Web.HttpUtility.UrlDecode(id);
                path = IOHelper.MapPath(FilePath + "/" + orgPath);
            }
            else
            {
                path = IOHelper.MapPath(FilePath);
            }

            var dirInfo = new DirectoryInfo(path);
            //check if it's a directory
            if (dirInfo.Attributes == FileAttributes.Directory)
            {
                //set the default to create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;
                //create action
                menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
                
                var hasChildren = dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0;
                //We can only delete folders if it doesn't have any children (folders or files)
                if (hasChildren == false)
                {
                    //delete action
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)), true);
                }

                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);
            }
            //if it's not a directory then we only allow to delete the item
            else
            {
                //delete action
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
            }

            return menu;
        }
    }
}
