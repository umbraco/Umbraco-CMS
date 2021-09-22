using System;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract IFileSystem FileSystem { get; }
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
        protected virtual void OnRenderFolderNode(ref TreeNode treeNode) {
            // TODO: This isn't the best way to ensure a noop process for clicking a node but it works for now.
            treeNode.AdditionalData["jsClickCallback"] = "javascript:void(0);";
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
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
                if (node != null)
                    nodes.Add(node);
            }

            //this is a hack to enable file system tree to support multiple file extension look-up
            //so the pattern both support *.* *.xml and xml,js,vb for lookups
            var files = FileSystem.GetFiles(path).Where(x =>
            {
                var extension = Path.GetExtension(x);

                if (Extensions.Contains("*"))
                    return true;
                
                return extension != null && Extensions.Contains(extension.Trim(Constants.CharArrays.Period), StringComparer.InvariantCultureIgnoreCase);
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

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any children
            root.HasChildren = GetTreeNodes(Constants.System.RootString, queryStrings).Any();
            return root;
        }

        protected virtual MenuItemCollection GetMenuForRootNode(FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;
            //create action
            menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
            //refresh action
            menu.Items.Add(new RefreshNode(Services.TextService, true));

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFolder(string path, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;
            //create action
            menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);

            var hasChildren = FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

            //We can only delete folders if it doesn't have any children (folders or files)
            if (hasChildren == false)
            {
                //delete action
                menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);
            }

            //refresh action
            menu.Items.Add(new RefreshNode(Services.TextService, true));

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFile(string path, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //if it's not a directory then we only allow to delete the item
            menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);

            return menu;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == Constants.System.RootString)
            {
                return GetMenuForRootNode(queryStrings);
            }

            var menu = new MenuItemCollection();

            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
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
