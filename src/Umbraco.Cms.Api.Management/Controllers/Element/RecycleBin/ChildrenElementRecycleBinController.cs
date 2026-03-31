using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// API controller responsible for retrieving child elements within the element recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenElementRecycleBinController : ElementRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenElementRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public ChildrenElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
    }

    /// <summary>
    /// Gets a paginated collection of child elements in the recycle bin for the specified parent.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent element in the recycle bin.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <returns>A paginated collection of element recycle bin items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ElementRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of elements in the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of elements that are children of the provided parent in the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<ElementRecycleBinItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentId, skip, take);
}
