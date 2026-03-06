using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

[ApiVersion("1.0")]
public class ChildrenElementTreeController : ElementTreeControllerBase
{
    [ActivatorUtilitiesConstructor]
    public ChildrenElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IElementStartNodeTreeFilterService treeFilterService,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : base(entityService, flagProviders, treeFilterService, elementPresentationFactory, elementPermissionFilterService)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public ChildrenElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IElementStartNodeTreeFilterService>(),
            elementPresentationFactory,
            elementPermissionFilterService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ElementTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of element tree child items.")]
    [EndpointDescription("Gets a paginated collection of element tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<ElementTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
