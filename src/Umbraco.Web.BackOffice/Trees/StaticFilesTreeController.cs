using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public StaticFilesTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IPhysicalFileSystem fileSystem,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IWebHostEnvironment webHostEnvironment)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _fileSystem = fileSystem;
        _menuItemCollectionFactory = menuItemCollectionFactory;
        _webHostEnvironment = webHostEnvironment;
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
            ? WebUtility.UrlDecode(id).TrimStart("/")
            : string.Empty;

        var nodes = new TreeNodeCollection();

        // Add App_Plugins && wwwroot folder if path is empty, as we are only returning root folders.
        if (path == string.Empty)
        {
            AddRootFolder(AppPlugins, queryStrings, nodes);
            AddRootFolder(Webroot, queryStrings, nodes);
        }
        else
        {
            if (path.StartsWith(Webroot, StringComparison.OrdinalIgnoreCase))
            {
                AddWebRootFiles(path, queryStrings, nodes);
            }
            else if (path.StartsWith(AppPlugins, StringComparison.OrdinalIgnoreCase))
            {
                AddPhysicalFiles(path, queryStrings, nodes);
            }
        }

        return nodes;
    }

    // We don't have any menu item options (such as create/delete/reload) & only use the root node to load a custom UI
    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings) =>
        _menuItemCollectionFactory.Create();

    private void AddRootFolder(string directory, FormCollection queryStrings, TreeNodeCollection nodes)
    {
        if (_fileSystem.DirectoryExists(directory) is false)
        {
            return;
        }

        var hasChildren = _fileSystem.GetFiles(directory).Any() || _fileSystem.GetDirectories(directory).Any();

        var name = Path.GetFileName(directory);
        TreeNode node = CreateTreeNode(WebUtility.UrlEncode(directory), "", queryStrings, name, Constants.Icons.Folder, hasChildren);
        nodes.Add(node);
    }

    private void AddPhysicalFiles(string path, FormCollection queryStrings, TreeNodeCollection nodes)
    {
        IEnumerable<string> files = _fileSystem.GetFiles(path)
            .Where(x => x.StartsWith(AppPlugins) || x.StartsWith(Webroot));

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            TreeNode node = CreateTreeNode(WebUtility.UrlEncode(file), path, queryStrings, name, Constants.Icons.DefaultIcon, false);
            nodes.Add(node);
        }
    }

    private void AddWebRootFiles(string path, FormCollection queryStrings, TreeNodeCollection nodes)
    {

        var calculatedPath = path.TrimStart(Webroot);
        IDirectoryContents files = _webHostEnvironment.WebRootFileProvider.GetDirectoryContents(calculatedPath);
        foreach (IFileInfo file in files)
        {
            TreeNode? node = null;
            if (file.IsDirectory)
            {
                if (file.PhysicalPath != null)
                {
                    var hasChildren = _fileSystem.GetFiles(file.PhysicalPath).Any() || _fileSystem.GetDirectories(file.PhysicalPath).Any();
                    node = CreateTreeNode(WebUtility.UrlEncode(string.Join("/", path, file.Name)), path, queryStrings, file.Name, Constants.Icons.Folder, hasChildren);
                }
            }
            else
            {
                node = CreateTreeNode(WebUtility.UrlEncode(string.Join("/", path, file.Name)), path, queryStrings, file.Name, Constants.Icons.DefaultIcon, false);
            }

            if (node != null)
            {
                nodes.Add(node);
            }
        }
    }
}
