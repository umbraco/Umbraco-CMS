// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for using the list of sections
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class SectionController : UmbracoAuthorizedJsonController
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IControllerFactory _controllerFactory;
    private readonly IDashboardService _dashboardService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ISectionService _sectionService;
    private readonly ITreeService _treeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public SectionController(
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        ILocalizedTextService localizedTextService,
        IDashboardService dashboardService,
        ISectionService sectionService,
        ITreeService treeService,
        IUmbracoMapper umbracoMapper,
        IControllerFactory controllerFactory,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _localizedTextService = localizedTextService;
        _dashboardService = dashboardService;
        _sectionService = sectionService;
        _treeService = treeService;
        _umbracoMapper = umbracoMapper;
        _controllerFactory = controllerFactory;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    public async Task<ActionResult<IEnumerable<Section>>> GetSections()
    {
        IEnumerable<ISection> sections =
            _sectionService.GetAllowedSections(_backofficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? 0);

        Section[] sectionModels = sections.Select(_umbracoMapper.Map<Section>).WhereNotNull().ToArray();

        // this is a bit nasty since we'll be proxying via the app tree controller but we sort of have to do that
        // since tree's by nature are controllers and require request contextual data
        var appTreeController =
            new ApplicationTreeController(_treeService, _sectionService, _localizedTextService, _controllerFactory, _actionDescriptorCollectionProvider)
            { ControllerContext = ControllerContext };

        IDictionary<string, IEnumerable<Tab<IDashboard>>> dashboards =
            _dashboardService.GetDashboards(_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser);

        //now we can add metadata for each section so that the UI knows if there's actually anything at all to render for
        //a dashboard for a given section, then the UI can deal with it accordingly (i.e. redirect to the first tree)
        foreach (Section? section in sectionModels)
        {
            var hasDashboards = section?.Alias is not null &&
                                dashboards.TryGetValue(section.Alias, out IEnumerable<Tab<IDashboard>>? dashboardsForSection) &&
                                dashboardsForSection.Any();
            if (hasDashboards)
            {
                continue;
            }

            // get the first tree in the section and get its root node route path
            ActionResult<TreeRootNode> sectionRoot =
                await appTreeController.GetApplicationTrees(section?.Alias, null, null);

            if (!(sectionRoot.Result is null))
            {
                return sectionRoot.Result;
            }

            if (section is not null)
            {
                section.RoutePath = GetRoutePathForFirstTree(sectionRoot.Value!);
            }
        }

        return sectionModels;
    }

    /// <summary>
    ///     Returns the first non root/group node's route path
    /// </summary>
    /// <param name="rootNode"></param>
    /// <returns></returns>
    private string? GetRoutePathForFirstTree(TreeRootNode rootNode)
    {
        if (!rootNode.IsContainer || !rootNode.ContainsTrees)
        {
            return rootNode.RoutePath;
        }

        if (rootNode.Children is not null)
        {
            foreach (TreeNode node in rootNode.Children)
            {
                if (node is TreeRootNode groupRoot)
                {
                    return GetRoutePathForFirstTree(groupRoot); //recurse to get the first tree in the group
                }

                return node.RoutePath;
            }
        }

        return string.Empty;
    }

    /// <summary>
    ///     Returns all the sections that the user has access to
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Section?> GetAllSections()
    {
        IEnumerable<ISection> sections = _sectionService.GetSections();
        IEnumerable<Section?> mapped = sections.Select(_umbracoMapper.Map<Section>);
        if (_backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.IsAdmin() ?? false)
        {
            return mapped;
        }

        return mapped.Where(x =>
                _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.AllowedSections.Contains(x?.Alias) ??
                false)
            .ToArray();
    }
}
