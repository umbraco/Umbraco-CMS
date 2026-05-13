using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

/// <summary>
/// API controller responsible for retrieving all versions of a specific element.
/// </summary>
[ApiVersion("1.0")]
public class AllElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IElementVersionPresentationFactory _elementVersionPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="elementVersionPresentationFactory">Factory for creating element version presentation models.</param>
    public AllElementVersionController(
        IElementVersionService elementVersionService,
        IElementVersionPresentationFactory elementVersionPresentationFactory)
    {
        _elementVersionService = elementVersionService;
        _elementVersionPresentationFactory = elementVersionPresentationFactory;
    }

    /// <summary>
    /// Gets a paginated collection of versions for a specific element.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="elementId">The unique identifier of the element to retrieve versions for.</param>
    /// <param name="culture">Optional culture filter for variant elements.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <returns>An <see cref="IActionResult"/> containing a paginated collection of element versions.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedViewModel<ElementVersionItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a paginated collection of versions for a specific element.")]
    [EndpointDescription("Gets a paginated collection of versions for a specific element and optional culture. Each result describes the version and includes details of the element type, editor, version date, and published status.")]
    public async Task<IActionResult> All(
        CancellationToken cancellationToken,
        [Required] Guid elementId,
        string? culture,
        int skip = 0,
        int take = 100)
    {
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await _elementVersionService.GetPagedContentVersionsAsync(elementId, culture, skip, take);

        if (attempt.Success is false)
        {
            return MapFailure(attempt.Status);
        }

        var pagedViewModel = new PagedViewModel<ElementVersionItemResponseModel>
        {
            Total = attempt.Result!.Total,
            Items = await _elementVersionPresentationFactory.CreateMultipleAsync(attempt.Result!.Items),
        };

        return Ok(pagedViewModel);
    }
}
