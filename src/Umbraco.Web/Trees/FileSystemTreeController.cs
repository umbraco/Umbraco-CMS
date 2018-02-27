using System;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using System.Web;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract IFileSystem2 FileSystem { get; }
        protected abstract string[] Extensions { get; }
        protected abstract string FileIcon { get; }

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
            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.Root.ToInvariantString()
                ? HttpUtility.UrlDecode(id).TrimStart("/")
                : "";

            var directories = FileSystem.GetDirectories(path);

            var nodes = new TreeNodeCollection();
            foreach (var directory in directories)
            {
                var hasChildren = FileSystem.GetFiles(directory).Any() || FileSystem.GetDirectories(directory).Any();

                var name = Path.GetFileName(directory);
                var node = CreateTreeNode(HttpUtility.UrlEncode(directory), path, queryStrings, name, "icon-folder", hasChildren);
                OnRenderFolderNode(ref node);
                if(node != null)
                    nodes.Add(node);
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            var files = FileSystem.GetFiles(path).Where(x =>
            {
                var extension = Path.GetExtension(x);
                return extension != null && Extensions.Contains(extension.Trim('.'), StringComparer.InvariantCultureIgnoreCase);
            });

            foreach (var file in files)
            {
                var withoutExt = Path.GetFileNameWithoutExtension(file);
                if (withoutExt.IsNullOrWhiteSpace()) continue;

                var name = Path.GetFileName(file);
                var node = CreateTreeNode(HttpUtility.UrlEncode(file), path, queryStrings, name, FileIcon, false);
                OnRenderFileNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            return nodes;
        }

        protected virtual MenuItemCollection GetMenuForRootNode(FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;
            //create action
            menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));
            //refresh action
            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFolder(string path, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;
            //create action
            menu.Items.Add<ActionNew>(Services.TextService.Localize(string.Format("actions/{0}", ActionNew.Instance.Alias)));

            var hasChildren = FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

            //We can only delete folders if it doesn't have any children (folders or files)
            if (hasChildren == false)
            {
                //delete action
                menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)), true);
            }

            //refresh action
            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize(string.Format("actions/{0}", ActionRefresh.Instance.Alias)), true);

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFile(string path, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //if it's not a directory then we only allow to delete the item
            menu.Items.Add<ActionDelete>(Services.TextService.Localize(string.Format("actions/{0}", ActionDelete.Instance.Alias)));

            return menu;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == Constants.System.Root.ToInvariantString())
            {
                return GetMenuForRootNode(queryStrings);
            }

            var menu = new MenuItemCollection();

            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.Root.ToInvariantString()
                ? HttpUtility.UrlDecode(id).TrimStart("/")
                : "";

            var isFile = FileSystem.FileExists(path);
            var isDirectory = FileSystem.DirectoryExists(path);

            if (isDirectory)
            {
                return GetMenuForFolder(path, queryStrings);
            }

            return isFile ? GetMenuForFile(path, queryStrings) : menu;
        }
    }
}
