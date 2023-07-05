using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[CoreTree]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[Tree(Constants.Applications.Settings, Constants.Trees.Scripts, TreeTitle = "Scripts", SortOrder = 10, TreeGroup = Constants.Trees.Groups.Templating)]
public class ScriptsTreeController : FileSystemTreeController
{
    private static readonly string[] ExtensionsStatic = { "js" };

    public ScriptsTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        FileSystems fileSystems,
        IEventAggregator eventAggregator)
        : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator) =>
        FileSystem = fileSystems.ScriptsFileSystem;

    protected override IFileSystem? FileSystem { get; }

    protected override string[] Extensions => ExtensionsStatic;

    protected override string FileIcon => "icon-script";
}
