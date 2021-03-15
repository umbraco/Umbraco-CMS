using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    public abstract class FileSystemTreeController : TreeController
    {
        protected FileSystemTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IEventAggregator eventAggregator
        )
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            MenuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected abstract IFileSystem FileSystem { get; }
        protected IMenuItemCollectionFactory MenuItemCollectionFactory { get; }
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

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
                ? WebUtility.UrlDecode(id).TrimStart("/")
                : "";

            var directories = FileSystem.GetDirectories(path);

            var nodes = new TreeNodeCollection();
            foreach (var directory in directories)
            {
                var hasChildren = FileSystem.GetFiles(directory).Any() || FileSystem.GetDirectories(directory).Any();

                var name = Path.GetFileName(directory);
                var node = CreateTreeNode(WebUtility.UrlEncode(directory), path, queryStrings, name, "icon-folder", hasChildren);
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
                var node = CreateTreeNode(WebUtility.UrlEncode(file), path, queryStrings, name, FileIcon, false);
                OnRenderFileNode(ref node);
                if (node != null)
                    nodes.Add(node);
            }

            return nodes;
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;

            //check if there are any children
            var treeNodesResult = GetTreeNodes(Constants.System.RootString, queryStrings);
            if (!(treeNodesResult.Result is null))
            {
                return treeNodesResult.Result;
            }
            root.HasChildren = treeNodesResult.Value.Any();
            return root;
        }

        protected virtual MenuItemCollection GetMenuForRootNode(FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;
            //create action
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
            //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFolder(string path, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //set the default to create
            menu.DefaultMenuAlias = ActionNew.ActionAlias;
            //create action
            menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);

            var hasChildren = FileSystem.GetFiles(path).Any() || FileSystem.GetDirectories(path).Any();

            //We can only delete folders if it doesn't have any children (folders or files)
            if (hasChildren == false)
            {
                //delete action
                menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true);
            }

            //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

        protected virtual MenuItemCollection GetMenuForFile(string path, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //if it's not a directory then we only allow to delete the item
            menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);

            return menu;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            //if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == Constants.System.RootString)
            {
                return GetMenuForRootNode(queryStrings);
            }

            var menu = MenuItemCollectionFactory.Create();

            var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
                ? WebUtility.UrlDecode(id).TrimStart("/")
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
