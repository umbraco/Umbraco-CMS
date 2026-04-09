using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// Provides API endpoints for moving media types within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class MoveMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveMediaTypeController"/> class with the specified media type service.
    /// </summary>
    /// <param name="mediaTypeService">The service used to manage media types.</param>
    public MoveMediaTypeController(IMediaTypeService mediaTypeService)
        => _mediaTypeService = mediaTypeService;

    /// <summary>
    /// Moves a media type identified by the provided Id to a different location.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="id">The unique identifier of the media type to move.</param>
    /// <param name="moveMediaTypeRequestModel">The request model containing the target location information.</param>
    /// <returns>An IActionResult indicating the result of the move operation.</returns>
    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a media type.")]
    [EndpointDescription("Moves a media type identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(
        CancellationToken cancellationToken,
        Guid id,
        MoveMediaTypeRequestModel moveMediaTypeRequestModel)
    {
        Attempt<IMediaType?, ContentTypeStructureOperationStatus> result = await _mediaTypeService.MoveAsync(id, moveMediaTypeRequestModel.Target?.Id);

        return result.Success
            ? Ok()
            : StructureOperationStatusResult(result.Status);
    }
}
