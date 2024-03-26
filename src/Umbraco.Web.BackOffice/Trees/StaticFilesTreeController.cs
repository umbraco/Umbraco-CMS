using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly CompositeFileProvider? _fileProvider;
    private readonly IWebHostEnvironment? _hostingEnvironment;
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
    private readonly Func<string, FormCollection, ActionResult<TreeNodeCollection>> _getTreeNodes;

    [Obsolete($"Use the constructor with {nameof(IFileProvider)} instead.")]
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
        _getTreeNodes = GetTreeNodesFromPhysicalFileSystem;
    }

    [ActivatorUtilitiesConstructor]
    public StaticFilesTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IPhysicalFileSystem fileSystem,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IWebHostEnvironment hostingEnvironment)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _fileSystem = fileSystem;

        _fileProvider = new CompositeFileProvider(
                hostingEnvironment.ContentRootFileProvider, // physical file provider
                hostingEnvironment.WebRootFileProvider); // packages file provider

        _menuItemCollectionFactory = menuItemCollectionFactory;
        _getTreeNodes = GetTreeNodesFromCompositeFieProvider;
    }

    // GetTreeNodes method calls a privat method that was assigned by a class constructor
    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings) => _getTreeNodes(id, queryStrings);

    // We don't have any menu item options (such as create/delete/reload) & only use the root node to load a custom UI
    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings) =>
        _menuItemCollectionFactory.Create();

    private ActionResult<TreeNodeCollection> GetTreeNodesFromCompositeFieProvider(string id, FormCollection queryStrings)
    {
        if (_fileProvider is null)
        {
            throw new ArgumentNullException(nameof(_fileProvider));
        }

        var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
            ? WebUtility.UrlDecode(id).TrimStart("/")
            : string.Empty;
        var nodes = new TreeNodeCollection();

        IEnumerable<IFileInfo> directories = _fileProvider.GetDirectoryContents(path).Where(i => i.IsDirectory);

        foreach (IFileInfo directory in directories)
        {
            if (directory == null)
            {
                continue;
            }

            // We don't want any other directories under the root node other than the ones serving static files - App_Plugins and wwwroot
            if (id == Constants.System.RootString && directory.Name != AppPlugins && directory.Name != Webroot)
            {
                continue;
            }

            var relPath = string.IsNullOrEmpty(path) ? directory.Name : $"{path}/{directory.Name}";

            // Get content of current path
            IDirectoryContents content = _fileProvider.GetDirectoryContents(relPath);
            var hasChildren = content.Any();

            TreeNode node = CreateTreeNode(
                WebUtility.UrlEncode(relPath),
                path,
                queryStrings,
                directory.Name,
                icon: Constants.Icons.Folder,
                hasChildren);

            if (node != null)
            {
                nodes.Add(node);
            }
        }

        // Get files in current directory
        IEnumerable<IFileInfo> files = _fileProvider.GetDirectoryContents(path).Where(i => !i.IsDirectory);

        // Only add the files inside App_Plugins or wwwroot
        if (id != Constants.System.RootString && (id.StartsWith(AppPlugins) || id.StartsWith(Webroot)))
        {
            foreach (IFileInfo file in files)
            {
                if (file == null)
                {
                    continue;
                }

                var name = file.Name;
                var relName = string.IsNullOrEmpty(path) ? name : $"{path}/{name}";

                TreeNode node = CreateTreeNode(
                    WebUtility.UrlEncode(relName),
                    path,
                    queryStrings,
                    name,
                    icon: Constants.Icons.DefaultIcon,
                    false);

                if (node != null)
                {
                    nodes.Add(node);
                }
            }
        }

        return nodes;
    }

    private Exception GetArgumentNullException() => new ArgumentNullException(nameof(_fileSystem));

    [Obsolete($"The metod supports getting tree nodes from physical file system. It is replaced by {nameof(GetTreeNodesFromCompositeFieProvider)}")]
    private ActionResult<TreeNodeCollection> GetTreeNodesFromPhysicalFileSystem(string id, FormCollection queryStrings)
    {
        if (_fileSystem is null)
        {
            throw new ArgumentNullException(nameof(_fileSystem));
        }

        var path = string.IsNullOrEmpty(id) == false && id != Constants.System.RootString
            ? WebUtility.UrlDecode(id).TrimStart("/")
            : string.Empty;

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
            TreeNode? node = CreateTreeNode(
                WebUtility.UrlEncode(directory),
                path,
                queryStrings,
                name,
                icon: Constants.Icons.Folder,
                hasChildren);

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
            TreeNode? node = CreateTreeNode(
                WebUtility.UrlEncode(file),
                path,
                queryStrings,
                name,
                icon: Constants.Icons.DefaultIcon,
                false);

            if (node != null)
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }
}
