using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.RecycleBin;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// API controller responsible for retrieving sibling elements in the element recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class SiblingsElementRecycleBinController : ElementRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsElementRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public SiblingsElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
    }

    /// <summary>
    /// Gets a collection of sibling elements in the recycle bin at the same level as the specified target.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the target element.</param>
    /// <param name="before">The number of sibling items to retrieve before the target.</param>
    /// <param name="after">The number of sibling items to retrieve after the target.</param>
    /// <param name="dataTypeId">Optional data type filter.</param>
    /// <returns>A subset collection of element recycle bin items.</returns>
    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<ElementRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets sibling elements in the recycle bin.")]
    [EndpointDescription("Gets a collection of sibling elements in the recycle bin at the same level as the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<ElementRecycleBinItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
        => await GetSiblings(target, before, after);
}
