using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiVersion("1.0")]
public class CreateMediaController : MediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaEditingPresentationFactory _mediaEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateMediaController(IMediaEditingService mediaEditingService, IMediaEditingPresentationFactory mediaEditingPresentationFactory, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaEditingService = mediaEditingService;
        _mediaEditingPresentationFactory = mediaEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateMediaRequestModel createRequestModel)
    {
        MediaCreateModel model = _mediaEditingPresentationFactory.MapCreateModel(createRequestModel);
        Attempt<IMedia?, ContentEditingOperationStatus> result = await _mediaEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyMediaController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
