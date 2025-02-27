using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class CreateDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IDocumentTypeEditingPresentationFactory _documentTypeEditingPresentationFactory;
    private readonly IContentTypeEditingService _contentTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDocumentTypeController(
        IDocumentTypeEditingPresentationFactory documentTypeEditingPresentationFactory,
        IContentTypeEditingService contentTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _documentTypeEditingPresentationFactory = documentTypeEditingPresentationFactory;
        _contentTypeEditingService = contentTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateDocumentTypeRequestModel requestModel)
    {
        ContentTypeCreateModel model = _documentTypeEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<IContentType?, ContentTypeOperationStatus> result = await _contentTypeEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : OperationStatusResult(result.Status);
    }
}
