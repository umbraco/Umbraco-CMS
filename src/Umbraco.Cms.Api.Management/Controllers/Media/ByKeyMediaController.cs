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

[ApiVersion("1.0")]
public class ByKeyMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaPresentationFactory _mediaPresentationFactory;

    public ByKeyMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IMediaPresentationFactory mediaPresentationFactory)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _mediaPresentationFactory = mediaPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
