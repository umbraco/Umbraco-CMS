using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

/// <summary>
/// Provides an API controller for permanently deleting all items from the media recycle bin in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class EmptyMediaRecycleBinController : MediaRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaService _mediaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyMediaRecycleBinController"/> class, responsible for handling requests to empty the media recycle bin.
    /// </summary>
    /// <param name="entityService">Service for managing and querying entities in the system.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office user security context.</param>
    /// <param name="mediaService">Service for managing media items.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    public EmptyMediaRecycleBinController(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaService mediaService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaService = mediaService;
    }

    /// <summary>
    /// Empties the media recycle bin by permanently deleting all media items in it.
    /// This operation cannot be undone.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    
    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Empties the media recycle bin.")]
    [EndpointDescription("Permanently deletes all media items in the recycle bin. This operation cannot be undone.")]
    public async Task<IActionResult> EmptyRecycleBin(CancellationToken cancellationToken)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.RecycleBin(),
            AuthorizationPolicies.MediaPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        OperationResult result = await _mediaService.EmptyRecycleBinAsync(CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
