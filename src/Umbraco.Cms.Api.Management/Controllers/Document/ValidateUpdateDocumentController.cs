using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.1")]
public class ValidateUpdateDocumentController : UpdateDocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;

    public ValidateUpdateDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory)
        : base(authorizationService)
    {
        _contentEditingService = contentEditingService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
    }

    [HttpPut("{id:guid}/validate")]
    [MapToApiVersion("1.1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateV1_1(CancellationToken cancellationToken, Guid id, ValidateUpdateDocumentRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            ValidateContentUpdateModel model = _documentEditingPresentationFactory.MapValidateUpdateModel(requestModel);
            Attempt<ContentValidationResult, ContentEditingOperationStatus> result = await _contentEditingService.ValidateUpdateAsync(id, model);

            return result.Success
                ? Ok()
                : DocumentEditingOperationStatusResult(result.Status, requestModel, result.Result);
        });
}
