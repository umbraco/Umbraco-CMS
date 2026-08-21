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

/// <summary>
/// Controller responsible for handling requests to move document types within the system.
/// Provides endpoints for relocating document type definitions.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class MoveDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeService">The service used to manage content types.</param>
    public MoveDocumentTypeController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a document type.")]
    [EndpointDescription("Moves a document type identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(
        CancellationToken cancellationToken,
        Guid id,
        MoveDocumentTypeRequestModel moveDocumentTypeRequestModel)
    {
        Attempt<IContentType?, ContentTypeStructureOperationStatus> result = await _contentTypeService.MoveAsync(id, moveDocumentTypeRequestModel.Target?.Id);

        return result.Success
            ? Ok()
            : StructureOperationStatusResult(result.Status);
    }
}
