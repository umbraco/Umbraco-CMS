using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
public class AllowedParentsDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    public AllowedParentsDocumentTypeController(IContentTypeService contentTypeService)
    {
        _contentTypeService = contentTypeService;
    }

    [HttpGet("{id:guid}/allowed-parents")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeAllowedParentsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AllowedParentsByKey(
        CancellationToken cancellationToken,
        Guid id)
    {
        Attempt<IEnumerable<Guid>, ContentTypeOperationStatus> attempt = await _contentTypeService.GetAllowedParentsAsync(id);
        if (attempt.Success is false)
        {
            return OperationStatusResult(attempt.Status);
        }

        var model = new DocumentTypeAllowedParentsResponseModel
        {
            AllowedParentIds = (attempt.Result ?? []).Select(x => new ReferenceByIdModel(x)).ToHashSet(),
        };

        return Ok(model);
    }
}
