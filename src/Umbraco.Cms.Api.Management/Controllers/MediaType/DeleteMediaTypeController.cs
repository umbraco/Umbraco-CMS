using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// Controller responsible for handling requests to delete media types in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class DeleteMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMediaTypeController"/> class, responsible for handling requests to delete media types.
    /// </summary>
    /// <param name="mediaTypeService">Service used to manage media types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    public DeleteMediaTypeController(IMediaTypeService mediaTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaTypeService = mediaTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes a media type identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the media type to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a media type.")]
    [EndpointDescription("Deletes a media type identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        ContentTypeOperationStatus status = await _mediaTypeService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));
        return OperationStatusResult(status);
    }
}
