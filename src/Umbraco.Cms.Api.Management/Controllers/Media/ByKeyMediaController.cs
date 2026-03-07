using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

    /// <summary>
    /// Provides API endpoints for managing media items identified by their unique key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Media.ByKeyMediaController"/> class, which manages media items by their unique key.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to media operations.</param>
    /// <param name="mediaEditingService">Service responsible for editing media items.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    public ByKeyMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IMediaPresentationFactory mediaPresentationFactory)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    /// <summary>
    /// Retrieves a media item by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the media item to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the media item if found and authorized;
    /// otherwise, returns <c>404 Not Found</c> if the item does not exist, or <c>403 Forbidden</c> if access is denied.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a media item.")]
    [EndpointDescription("Gets a media item identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.WithKeys(id),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IMedia? media = await _mediaEditingService.GetAsync(id);
        if (media == null)
        {
            return MediaNotFound();
        }

        MediaResponseModel model = _mediaPresentationFactory.CreateResponseModel(media);
        return Ok(model);
    }
}
