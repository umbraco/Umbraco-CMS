using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

/// <summary>
/// Controller responsible for managing media items in the original parent recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class OriginalParentMediaRecycleBinController : MediaRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;
    private readonly IMediaRecycleBinQueryService _mediaRecycleBinQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginalParentMediaRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within the Umbraco system.</param>
    /// <param name="authorizationService">Service used to authorize user access to media recycle bin operations.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models for API responses.</param>
    /// <param name="mediaRecycleBinQueryService">Service for querying media items in the recycle bin.</param>
    public OriginalParentMediaRecycleBinController(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IMediaPresentationFactory mediaPresentationFactory,
        IMediaRecycleBinQueryService mediaRecycleBinQueryService)
        : base(entityService, mediaPresentationFactory)
    {
        _authorizationService = authorizationService;
        _mediaPresentationFactory = mediaPresentationFactory;
        _mediaRecycleBinQueryService = mediaRecycleBinQueryService;
    }

    /// <summary>
    /// Retrieves the original parent of a media item that has been moved to the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the media item whose original parent is to be retrieved.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="ReferenceByIdModel"/> with the original parent's ID if it exists, or <c>null</c> if the original parent is the root.
    /// Returns <see cref="StatusCodes.Status404NotFound"/> if the media item does not exist, or <see cref="StatusCodes.Status400BadRequest"/> for invalid requests.
    /// </returns>
    [HttpGet("{id:guid}/original-parent")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ReferenceByIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets the original parent of a media item in the recycle bin.")]
    [EndpointDescription("Gets the original parent location of a media item before it was moved to the recycle bin.")]
    public async Task<IActionResult> OriginalParent(
        CancellationToken cancellationToken,
        Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.RecycleBin(),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IMediaEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _mediaRecycleBinQueryService.GetOriginalParentAsync(id);
        return getParentAttempt.Success switch
        {
            true when getParentAttempt.Status == RecycleBinQueryResultType.Success
                => Ok(new ReferenceByIdModel(getParentAttempt.Result!.Key)),
            true when getParentAttempt.Status == RecycleBinQueryResultType.ParentIsRoot
                => Ok(null),
            _ => MapAttemptFailure(getParentAttempt.Status),
        };
    }

    private IActionResult MapAttemptFailure(RecycleBinQueryResultType status)
        => MapRecycleBinQueryAttemptFailure(status, "media item");
}
