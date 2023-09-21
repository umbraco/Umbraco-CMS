using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class ByKeyMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaPresentationModelFactory _mediaPresentationModelFactory;

    public ByKeyMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IMediaPresentationModelFactory mediaPresentationModelFactory)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _mediaPresentationModelFactory = mediaPresentationModelFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { id },
            $"New{AuthorizationPolicies.MediaPermissionByResource}");

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IMedia? media = await _mediaEditingService.GetAsync(id);
        if (media == null)
        {
            return MediaNotFound();
        }

        MediaResponseModel model = await _mediaPresentationModelFactory.CreateResponseModelAsync(media);
        return Ok(model);
    }
}
