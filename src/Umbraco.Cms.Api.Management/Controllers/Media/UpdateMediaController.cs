using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class UpdateMediaController : UpdateMediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaEditingPresentationFactory _mediaEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IMediaEditingPresentationFactory mediaEditingPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _mediaEditingService = mediaEditingService;
        _mediaEditingPresentationFactory = mediaEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMediaRequestModel requestModel)
        => await HandleRequest(id, async () =>
        {
            MediaUpdateModel model = _mediaEditingPresentationFactory.MapUpdateModel(requestModel);
            Attempt<MediaUpdateResult, ContentEditingOperationStatus> result =
                await _mediaEditingService.UpdateAsync(id, model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? Ok()
                : ContentEditingOperationStatusResult(result.Status);
        });
}
