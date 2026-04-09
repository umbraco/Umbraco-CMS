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
/// API controller responsible for handling operations related to copying media types in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class CopyMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyMediaTypeController"/> class with the specified media type service.
    /// </summary>
    /// <param name="mediaTypeService">The service used to manage media types.</param>
    public CopyMediaTypeController(IMediaTypeService mediaTypeService)
        => _mediaTypeService = mediaTypeService;

    /// <summary>
    /// Creates a duplicate of an existing media type identified by the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the media type to copy.</param>
    /// <param name="copyMediaTypeRequestModel">The request model containing details for the copy operation, such as the target location.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the copy operation. Returns <c>201 Created</c> with the new media type if successful, or a <see cref="ProblemDetails"/> response with <c>400 Bad Request</c> or <c>404 Not Found</c> if the operation fails.
    /// </returns>
    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Copies a media type.")]
    [EndpointDescription("Creates a duplicate of an existing media type identified by the provided Id.")]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id,
        CopyMediaTypeRequestModel copyMediaTypeRequestModel)
    {
        Attempt<IMediaType?, ContentTypeStructureOperationStatus> result = await _mediaTypeService.CopyAsync(id, copyMediaTypeRequestModel.Target?.Id);

        return result.Success
            ? CreatedAtId<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : StructureOperationStatusResult(result.Status);
    }
}
