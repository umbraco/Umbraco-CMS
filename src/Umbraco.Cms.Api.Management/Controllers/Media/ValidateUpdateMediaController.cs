using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class ValidateUpdateMediaController : UpdateMediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaEditingPresentationFactory _mediaEditingPresentationFactory;

    public ValidateUpdateMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IMediaEditingPresentationFactory mediaEditingPresentationFactory)
        : base(authorizationService)
    {
        _mediaEditingService = mediaEditingService;
        _mediaEditingPresentationFactory = mediaEditingPresentationFactory;
    }

    [HttpPut("{id:guid}/validate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken, Guid id, UpdateMediaRequestModel requestModel)
        => await HandleRequest(id, async () =>
        {
            MediaUpdateModel model = _mediaEditingPresentationFactory.MapUpdateModel(requestModel);
            Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await _mediaEditingService.ValidateUpdateAsync(id, model);

            return result.Success
                ? Ok()
                : MediaEditingOperationStatusResult(result.Status, requestModel, result.Result);
        });
}
