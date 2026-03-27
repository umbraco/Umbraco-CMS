using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

/// <summary>
/// Provides API endpoints for managing the allowed parent document types of a document type.
/// </summary>
[ApiVersion("1.0")]
public class AllowedParentsDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedParentsDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeService">The service used to manage content types.</param>
    public AllowedParentsDocumentTypeController(IContentTypeService contentTypeService)
    {
        _contentTypeService = contentTypeService;
    }

    /// <summary>
    /// Retrieves the collection of document types that can be used as parents for the specified document type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique key (GUID) identifying the document type whose allowed parents are to be retrieved.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="DocumentTypeAllowedParentsResponseModel"/> with the allowed parent document types if found;
    /// otherwise, a status result indicating the error (such as not found).
    /// </returns>
    [HttpGet("{id:guid}/allowed-parents")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentTypeAllowedParentsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets allowed parent document types.")]
    [EndpointDescription("Gets a collection of document types that are allowed as parents of the specified document type.")]
    public async Task<IActionResult> AllowedParentsByKey(
        CancellationToken cancellationToken,
        Guid id)
    {
        Attempt<IEnumerable<Guid>, ContentTypeOperationStatus> attempt = await _contentTypeService.GetAllowedParentKeysAsync(id);
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
