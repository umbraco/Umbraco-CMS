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
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentEditingFactory _documentEditingFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateDocumentController(
        IContentEditingService contentEditingService,
        IDocumentEditingFactory documentEditingFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentEditingService = contentEditingService;
        _documentEditingFactory = documentEditingFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, DocumentUpdateRequestModel updateRequestModel)
    {
        IContent? content = await _contentEditingService.GetAsync(key);
        if (content == null)
        {
            return NotFound();
        }

        ContentUpdateModel model = _documentEditingFactory.MapUpdateModel(updateRequestModel);
        Attempt<IContent, ContentEditingOperationStatus> result = await _contentEditingService.UpdateAsync(content, model, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
