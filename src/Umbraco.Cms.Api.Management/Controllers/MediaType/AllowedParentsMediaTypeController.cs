using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

    /// <summary>
    /// Provides API endpoints for managing the allowed parent media types of a media type.
    /// </summary>
[ApiVersion("1.0")]
public class AllowedParentsMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedParentsMediaTypeController"/> class with the specified media type service.
    /// </summary>
    /// <param name="mediaTypeService">An instance of <see cref="IMediaTypeService"/> used to manage media types.</param>
    public AllowedParentsMediaTypeController(IMediaTypeService mediaTypeService)
    {
        _mediaTypeService = mediaTypeService;
    }

    /// <summary>
    /// Retrieves the media types that can be used as parents for the specified media type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the media type whose allowed parent types are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="MediaTypeAllowedParentsResponseModel"/> listing the allowed parent media types, or a 404 if not found.</returns>
    [HttpGet("{id:guid}/allowed-parents")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaTypeAllowedParentsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets allowed parent media types.")]
    [EndpointDescription("Gets a collection of media types that are allowed as parents of the specified media type.")]
    public async Task<IActionResult> AllowedParentsByKey(
        CancellationToken cancellationToken,
        Guid id)
    {
        Attempt<IEnumerable<Guid>, ContentTypeOperationStatus> attempt = await _mediaTypeService.GetAllowedParentKeysAsync(id);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        var model = new MediaTypeAllowedParentsResponseModel
        {
            AllowedParentIds = (attempt.Result ?? []).Select(x => new ReferenceByIdModel(x)).ToHashSet(),
        };

        return Ok(model);
    }
}
