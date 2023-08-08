using Asp.Versioning;
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

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateDocumentTypeRequestModel requestModel)
    {
        ContentTypeCreateModel model = _documentTypeEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<IContentType?, ContentTypeOperationStatus> result = await _contentTypeEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : OperationStatusResult(result.Status);
    }
}
