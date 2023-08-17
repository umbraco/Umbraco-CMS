using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiVersion("1.0")]
public class CreateMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeEditingPresentationFactory _mediaTypeEditingPresentationFactory;
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateMediaTypeController(
        IMediaTypeEditingPresentationFactory mediaTypeEditingPresentationFactory,
        IMediaTypeEditingService mediaTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaTypeEditingPresentationFactory = mediaTypeEditingPresentationFactory;
        _mediaTypeEditingService = mediaTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateMediaTypeRequestModel requestModel)
    {
        MediaTypeCreateModel model = _mediaTypeEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await _mediaTypeEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : OperationStatusResult(result.Status);
    }
}
