using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// API controller responsible for retrieving root-level elements in the element recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class RootElementRecycleBinController : ElementRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootElementRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public RootElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
    }

    /// <summary>
    /// Gets a paginated collection of elements at the root level of the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <returns>A paginated collection of element recycle bin items.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ElementRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets elements at the root of the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of elements at the root level of the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<ElementRecycleBinItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
