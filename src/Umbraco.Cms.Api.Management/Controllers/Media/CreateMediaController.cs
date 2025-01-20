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
public class CreateMediaController : CreateMediaControllerBase
{
    private readonly IMediaEditingPresentationFactory _mediaEditingPresentationFactory;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingPresentationFactory mediaEditingPresentationFactory,
        IMediaEditingService mediaEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _mediaEditingPresentationFactory = mediaEditingPresentationFactory;
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateMediaRequestModel requestModel)
        => await HandleRequest(requestModel.Parent?.Id, async () =>
        {
            MediaCreateModel model = _mediaEditingPresentationFactory.MapCreateModel(requestModel);
            Attempt<MediaCreateResult, ContentEditingOperationStatus> result = await _mediaEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? CreatedAtId<ByKeyMediaController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
                : MediaEditingOperationStatusResult(result.Status, requestModel, result.Result.ValidationResult);
        });
}
