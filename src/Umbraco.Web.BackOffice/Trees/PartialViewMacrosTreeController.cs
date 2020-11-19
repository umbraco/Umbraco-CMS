using Microsoft.AspNetCore.Authorization;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Trees
{
    /// <summary>
    /// Tree for displaying partial view macros in the developer app
    /// </summary>
    [Tree(Constants.Applications.Settings, Constants.Trees.PartialViewMacros, SortOrder = 8, TreeGroup = Constants.Trees.Groups.Templating)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessPartialViewMacros)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class PartialViewMacrosTreeController : PartialViewsTreeController
    {
        protected override IFileSystem FileSystem { get; }

        private static readonly string[] ExtensionsStatic = {"cshtml"};

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-article";

        public PartialViewMacrosTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IFileSystems fileSystems)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, fileSystems)
        {
            FileSystem = fileSystems.MacroPartialsFileSystem;
        }
    }
}
