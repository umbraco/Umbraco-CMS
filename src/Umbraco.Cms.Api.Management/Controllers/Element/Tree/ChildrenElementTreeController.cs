using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

/// <summary>
/// API controller responsible for retrieving child element tree items.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenElementTreeController : ElementTreeControllerBase
{
    /// <inheritdoc />
    public ChildrenElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : base(entityService, flagProviders, userStartNodeEntitiesService, dataTypeService, appCaches, backOfficeSecurityAccessor, elementPresentationFactory, elementPermissionFilterService)
    {
    }

    /// <summary>
    /// Gets a paginated collection of child element tree items for the specified parent.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent element.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <param name="foldersOnly">Whether to return only folder items.</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the element items.</param>
    /// <returns>A paginated collection of child element tree items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ElementTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of element tree child items.")]
    [EndpointDescription("Gets a paginated collection of element tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<ElementTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false, Guid? dataTypeId = null)
    {
        RenderFoldersOnly(foldersOnly);
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetChildren(parentId, skip, take);
    }
}
