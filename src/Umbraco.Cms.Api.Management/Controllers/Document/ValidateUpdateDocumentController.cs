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

[ApiVersion("1.1")]
public class ValidateUpdateDocumentController : UpdateDocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
    public ValidateUpdateDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory)
        : this(
              authorizationService,
              contentEditingService,
              documentEditingPresentationFactory,
              StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurityAccessor>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public ValidateUpdateDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _contentEditingService = contentEditingService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
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
            Attempt<ContentValidationResult, ContentEditingOperationStatus> result =
                await _contentEditingService.ValidateUpdateAsync(
                    id,
                    model,
                    CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? Ok()
                : DocumentEditingOperationStatusResult(result.Status, requestModel, result.Result);
        });
}
