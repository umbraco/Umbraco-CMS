using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class CopyDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public CopyDocumentTypeController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id,
        CopyDocumentTypeRequestModel copyDocumentTypeRequestModel)
    {
        Attempt<IContentType?, ContentTypeStructureOperationStatus> result = await _contentTypeService.CopyAsync(id, copyDocumentTypeRequestModel.Target?.Id);

        return result.Success
            ? CreatedAtId<ByKeyDocumentTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : StructureOperationStatusResult(result.Status);
    }
}
