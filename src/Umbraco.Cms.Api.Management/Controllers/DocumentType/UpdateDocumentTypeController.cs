using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class UpdateDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IDocumentTypeEditingPresentationFactory _documentTypeEditingPresentationFactory;
    private readonly IContentTypeEditingService _contentTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentTypeService _contentTypeService;

    public UpdateDocumentTypeController(
        IDocumentTypeEditingPresentationFactory documentTypeEditingPresentationFactory,
        IContentTypeEditingService contentTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeService contentTypeService)
    {
        _documentTypeEditingPresentationFactory = documentTypeEditingPresentationFactory;
        _contentTypeEditingService = contentTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentTypeService = contentTypeService;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateDocumentTypeRequestModel requestModel)
    {
        IContentType? contentType = await _contentTypeService.GetAsync(id);
        if (contentType is null)
        {
            return NotFound();
        }

        ContentTypeUpdateModel model = _documentTypeEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<IContentType?, ContentTypeOperationStatus> result = await _contentTypeEditingService.UpdateAsync(contentType, model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }
}
