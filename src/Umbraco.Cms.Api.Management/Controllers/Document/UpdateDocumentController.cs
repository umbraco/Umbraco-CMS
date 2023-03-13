using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class UpdateDocumentController : DocumentControllerBase
{
    private readonly IContentService _contentService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateDocumentController(
        IContentService contentService,
        IContentEditingService contentEditingService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentService = contentService;
        _contentEditingService = contentEditingService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, UpdateDocumentRequestModel requestModel)
    {
        IContent? content = _contentService.GetById(key);
        if (content == null)
        {
            return NotFound();
        }

        ContentUpdateModel model = _documentEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<IContent, ContentEditingOperationStatus> result = await _contentEditingService.UpdateAsync(content, model, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
