using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class ValidateCreateDocumentController : CreateDocumentControllerBase
{
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ValidateCreateDocumentController(
        IAuthorizationService authorizationService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("validate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Validates creating a document.")]
    [EndpointDescription("Validates the request model for creating a new document without actually creating it.")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken, CreateDocumentRequestModel requestModel)
        => await HandleRequest(requestModel, async () =>
        {
            ContentCreateModel model = _documentEditingPresentationFactory.MapCreateModel(requestModel);
            Attempt<ContentValidationResult, ContentEditingOperationStatus> result =
                await _contentEditingService.ValidateCreateAsync(
                    model,
                    CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? Ok()
                : DocumentEditingOperationStatusResult(result.Status, requestModel, result.Result);
        });
}
