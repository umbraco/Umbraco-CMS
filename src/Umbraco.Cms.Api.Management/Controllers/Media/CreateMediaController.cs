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

public class CreateMediaController : MediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaEditingFactory _mediaEditingFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateMediaController(IMediaEditingService mediaEditingService, IMediaEditingFactory mediaEditingFactory, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaEditingService = mediaEditingService;
        _mediaEditingFactory = mediaEditingFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(MediaCreateRequestModel createRequestModel)
    {
        MediaCreateModel model = _mediaEditingFactory.MapCreateModel(createRequestModel);
        Attempt<IMedia?, ContentEditingOperationStatus> result = await _mediaEditingService.CreateAsync(model, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyMediaController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
