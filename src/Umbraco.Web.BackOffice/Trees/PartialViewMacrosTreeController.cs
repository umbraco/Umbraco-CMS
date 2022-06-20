using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Trees;

/// <summary>
///     Tree for displaying partial view macros in the developer app
/// </summary>
[Tree(Constants.Applications.Settings, Constants.Trees.PartialViewMacros, SortOrder = 8, TreeGroup = Constants.Trees.Groups.Templating)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessPartialViewMacros)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class PartialViewMacrosTreeController : PartialViewsTreeController
{
    private static readonly string[] ExtensionsStatic = { "cshtml" };

    public PartialViewMacrosTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        FileSystems fileSystems,
        IEventAggregator eventAggregator)
        : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, fileSystems, eventAggregator) =>
        FileSystem = fileSystems.MacroPartialsFileSystem;

    protected override IFileSystem? FileSystem { get; }

    protected override string[] Extensions => ExtensionsStatic;

    protected override string FileIcon => "icon-article";
}
