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
/// API controller responsible for handling operations related to copying document types in Umbraco.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class CopyDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeService">An instance of <see cref="IContentTypeService"/> used to manage content types.</param>
    public CopyDocumentTypeController(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    /// <summary>
    /// Creates a duplicate of an existing document type specified by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document type to copy.</param>
    /// <param name="copyDocumentTypeRequestModel">The details for the copy operation, including the target location.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the copy operation.</returns>
    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Copies a document type.")]
    [EndpointDescription("Creates a duplicate of an existing document type identified by the provided Id.")]
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
