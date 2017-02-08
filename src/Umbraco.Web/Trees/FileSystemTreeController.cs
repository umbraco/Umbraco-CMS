using System;
using System.IO;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract string FilePath { get; }
        protected abstract string FileSearchPattern { get; }
        protected abstract string FileIcon { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        /// <param name="xNode"></param>
        protected virtual void OnRenderFileNode(ref TreeNode treeNode) { }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        /// <param name="xNode"></param>
        protected virtual void OnRenderFolderNode(ref TreeNode treeNode) { }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            string orgPath = "";
            string path = "";
            if (!string.IsNullOrEmpty(id) && id != "-1")
            {
                orgPath = System.Web.HttpUtility.UrlDecode(id);
                path = IOHelper.MapPath(FilePath + "/" + orgPath);
                orgPath += "/";
            }
            else
            {
                path = IOHelper.MapPath(FilePath);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();

            var nodes = new TreeNodeCollection();
            foreach (DirectoryInfo dir in dirInfos)
            {
                if ((dir.Attributes & FileAttributes.Hidden) == 0)
                {
                    var HasChildren = dir.GetFiles().Length > 0 || dir.GetDirectories().Length > 0;
                    var node = CreateTreeNode(System.Web.HttpUtility.UrlEncode(orgPath + dir.Name), orgPath, queryStrings, dir.Name, "icon-folder", HasChildren);

                    OnRenderFolderNode(ref node);
                    if(node != null)
                        nodes.Add(node);
                }
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            string[] allowedExtensions = new string[0];
            bool filterByMultipleExtensions = FileSearchPattern.Contains(",");
            FileInfo[] fileInfo;

            if (filterByMultipleExtensions)
            {
                fileInfo = dirInfo.GetFiles();
                allowedExtensions = FileSearchPattern.ToLower().Split(',');
            }
            else
                fileInfo = dirInfo.GetFiles(FileSearchPattern);

            foreach (FileInfo file in fileInfo)
            {
                if ((file.Attributes & FileAttributes.Hidden) == 0)
                {
                    if (filterByMultipleExtensions && Array.IndexOf<string>(allowedExtensions, file.Extension.ToLower().Trim('.')) < 0)
                        continue;

                    var node = CreateTreeNode(System.Web.HttpUtility.UrlEncode(orgPath + file.Name), orgPath, queryStrings, file.Name, FileIcon, false);

                    OnRenderFileNode(ref node);

                    if(node != null)
                        nodes.Add(node);
                }
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
                //refresh action
                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);

                var hasChildren = dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0;
                //We can only delete folders if it doesn't have any children (folders or files)
                if (hasChildren == false)
                {
                    //delete action
                    menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));
                }
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
