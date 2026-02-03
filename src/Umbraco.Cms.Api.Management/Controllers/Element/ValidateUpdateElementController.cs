using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class ValidateUpdateElementController : UpdateElementControllerBase
{
    private readonly IElementEditingPresentationFactory _elementEditingPresentationFactory;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ValidateUpdateElementController(
        IAuthorizationService authorizationService,
        IElementEditingPresentationFactory elementEditingPresentationFactory,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _elementEditingPresentationFactory = elementEditingPresentationFactory;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/validate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken, Guid id, ValidateUpdateElementRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            ValidateElementUpdateModel model = _elementEditingPresentationFactory.MapValidateUpdateModel(requestModel);
            Attempt<ContentValidationResult, ContentEditingOperationStatus> result =
                await _elementEditingService.ValidateUpdateAsync(id, model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? Ok()
                : ContentEditingOperationStatusResult(result.Status);
        });
}
