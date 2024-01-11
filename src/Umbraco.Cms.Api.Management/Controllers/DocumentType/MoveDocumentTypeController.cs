using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class MoveDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public MoveDocumentTypeController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    [HttpPost("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(Guid id, MoveDocumentTypeRequestModel moveDocumentTypeRequestModel)
    {
        IContentType? source = await _contentTypeService.GetAsync(id);
        if (source is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        Attempt<IContentType, ContentTypeStructureOperationStatus> result = await _contentTypeService.MoveAsync(source, moveDocumentTypeRequestModel.TargetId);

        return result.Success
            ? Ok()
            : StructureOperationStatusResult(result.Status);
    }
}
