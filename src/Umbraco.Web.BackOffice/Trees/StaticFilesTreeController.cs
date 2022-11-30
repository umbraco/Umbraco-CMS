using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[Tree(Constants.Applications.Settings, "staticFiles", TreeTitle = "Static Files", TreeUse = TreeUse.Dialog)]
public class StaticFilesTreeController : TreeController
{
    private const string AppPlugins = "App_Plugins";
    private const string Webroot = "wwwroot";
    private readonly IFileSystem _fileSystem;
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

    public StaticFilesTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IPhysicalFileSystem fileSystem,
        IMenuItemCollectionFactory menuItemCollectionFactory)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _fileSystem = fileSystem;
        _menuItemCollectionFactory = menuItemCollectionFactory;
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
            ? WebUtility.UrlDecode(id).TrimStart("/")
            : "";

        var nodes = new TreeNodeCollection();
        IEnumerable<string> directories = _fileSystem.GetDirectories(path);

        foreach (var directory in directories)
        {
            // We don't want any other directories under the root node other than the ones serving static files - App_Plugins and wwwroot
            if (id == Constants.System.RootString && directory != AppPlugins && directory != Webroot)
            {
                continue;
            }

            var hasChildren = _fileSystem.GetFiles(directory).Any() || _fileSystem.GetDirectories(directory).Any();

            var name = Path.GetFileName(directory);
            TreeNode? node = CreateTreeNode(WebUtility.UrlEncode(directory), path, queryStrings, name,
                Constants.Icons.Folder, hasChildren);

            if (node != null)
            {
                nodes.Add(node);
            }
        }

        // Only get the files inside App_Plugins and wwwroot
        IEnumerable<string> files = _fileSystem.GetFiles(path)
            .Where(x => x.StartsWith(AppPlugins) || x.StartsWith(Webroot));

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            TreeNode? node = CreateTreeNode(WebUtility.UrlEncode(file), path, queryStrings, name,
                Constants.Icons.DefaultIcon, false);

            if (node != null)
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    // We don't have any menu item options (such as create/delete/reload) & only use the root node to load a custom UI
    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings) =>
        _menuItemCollectionFactory.Create();
}
