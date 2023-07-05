using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[CoreTree]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[Tree(Constants.Applications.Settings, Constants.Trees.Stylesheets, TreeTitle = "Stylesheets", SortOrder = 9, TreeGroup = Constants.Trees.Groups.Templating)]
public class StylesheetsTreeController : FileSystemTreeController
{
    private static readonly string[] ExtensionsStatic = { "css" };

    public StylesheetsTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        FileSystems fileSystems,
        IEventAggregator eventAggregator)
        : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator) =>
        FileSystem = fileSystems.StylesheetsFileSystem;

    protected override IFileSystem? FileSystem { get; }

    protected override string[] Extensions => ExtensionsStatic;

    protected override string FileIcon => "icon-brackets";
}
