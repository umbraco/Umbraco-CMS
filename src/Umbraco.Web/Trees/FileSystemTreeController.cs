using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web._Legacy.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected abstract IFileSystem FileSystem { get; }
        protected abstract string[] Extensions { get; }
        protected abstract string FileIcon { get; }
        protected abstract bool EnableCreateOnFolder { get; }

        /// <summary>
        /// Inheritors can override this method to modify the file node that is created.
        /// </summary>
        protected virtual void OnRenderFileNode(ref TreeNode treeNode)
        { }

        /// <summary>
        /// Inheritors can override this method to modify the folder node that is created.
        /// </summary>
        protected virtual void OnRenderFolderNode(ref TreeNode treeNode)
        { }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.Root.ToInvariantString()
                ? HttpUtility.UrlDecode(id).TrimStart("/")
                : "";

            var directories = FileSystem.GetDirectories(path);

            foreach (var directory in directories)
            {
                var hasChildren = FileSystem.GetFiles(directory).Any() || FileSystem.GetDirectories(directory).Any();

                var name = Path.GetFileName(directory);
                var node = CreateTreeNode(HttpUtility.UrlEncode(directory), path, queryStrings, name, "icon-folder", hasChildren);
                OnRenderFolderNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            var files = FileSystem.GetFiles(path).Where(x =>
            {
                var extension = Path.GetExtension(x);
                return extension != null && Extensions.Contains(extension.Trim('.'), StringComparer.InvariantCultureIgnoreCase);

                // fixme - should we filter out hidden files? but then, FileSystem does not support attributes!
            });

            foreach (var file in files)
            {
                var withoutExt = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrWhiteSpace(withoutExt)) continue;

                var name = Path.GetFileName(file);
                var node = CreateTreeNode(HttpUtility.UrlEncode(file), path, queryStrings, name, FileIcon, false);

                OnRenderFileNode(ref node);

                if (node != null)
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

            // if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == Constants.System.Root.ToInvariantString())
            {
                GetMenuForRootNode(menu, queryStrings);
            }
            else
            {
                var path = string.IsNullOrEmpty(id) == false && id != Constants.System.Root.ToInvariantString()
                    ? HttpUtility.UrlDecode(id).TrimStart("/")
                    : "";
                var isFile = FileSystem.FileExists(path);
                var isDirectory = FileSystem.DirectoryExists(path);

                if (isDirectory)
                    GetMenuForFolder(menu, path, id, queryStrings);
                else if (isFile)
                    GetMenuForFile(menu, path, id, queryStrings);
            }

            OnAfterRenderMenu(menu, id, queryStrings);

            return menu;
        }

        protected virtual void GetMenuForRootNode(MenuItemCollection menu, FormDataCollection queryStrings)
        {
            // default create
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;

            // create action
            //Since we haven't implemented anything for file systems in angular, this needs to be converted to
            //use the legacy format
            menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                .ConvertLegacyFileSystemMenuItem("", "init" + TreeAlias, queryStrings.GetValue<string>("application"));

            // refresh action
            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
        }

        protected virtual void GetMenuForFolder(MenuItemCollection menu, string path, string id, FormDataCollection queryStrings)
        {
            if (EnableCreateOnFolder)
            {
                // default create
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // create action
                //Since we haven't implemented anything for file systems in angular, this needs to be converted to
                //use the legacy format
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    .ConvertLegacyFileSystemMenuItem(id, TreeAlias + "Folder", queryStrings.GetValue<string>("application"));
            }

            // delete action
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias), true)
                .ConvertLegacyFileSystemMenuItem(id, TreeAlias + "Folder", queryStrings.GetValue<string>("application"));

            // refresh action
            menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
        }

        protected virtual void GetMenuForFile(MenuItemCollection menu, string path, string id, FormDataCollection queryStrings)
        {
            // delete action
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias), true)
                .ConvertLegacyFileSystemMenuItem(id, TreeAlias, queryStrings.GetValue<string>("application"));
        }

        protected virtual void OnBeforeRenderMenu(MenuItemCollection menu, string id, FormDataCollection queryStrings)
        { }

        protected virtual void OnAfterRenderMenu(MenuItemCollection menu, string id, FormDataCollection queryStrings)
        { }
    }
}
