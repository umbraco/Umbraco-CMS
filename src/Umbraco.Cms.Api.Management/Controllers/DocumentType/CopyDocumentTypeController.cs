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
public class CopyDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public CopyDocumentTypeController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Copy(Guid id, CopyDocumentTypeRequestModel copyDocumentTypeRequestModel)
    {
        IContentType? source = await _contentTypeService.GetAsync(id);
        if (source is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        Attempt<IContentType, ContentTypeStructureOperationStatus> result = await _contentTypeService.CopyAsync(source, copyDocumentTypeRequestModel.TargetId);

        return result.Success
            ? CreatedAtAction<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : StructureOperationStatusResult(result.Status);
    }
}
