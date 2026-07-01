using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

/// <summary>
/// Controller for managing document version operations by document key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.DocumentVersion.ByKeyDocumentVersionController"/> class.
    /// </summary>
    /// <param name="contentVersionService">An instance of <see cref="IContentVersionService"/> used to manage content versions.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    public ByKeyDocumentVersionController(
        IContentVersionService contentVersionService,
        IUmbracoMapper umbracoMapper)
    {
        _contentVersionService = contentVersionService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a specific document version by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document version to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the document version details—including document type, editor, version date, and published status—if found;
    /// otherwise, a <see cref="ProblemDetails"/> response indicating why the version could not be retrieved (e.g., not found or invalid request).
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentVersionResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a specific document version.")]
    [EndpointDescription("Gets a specific document version by its Id. If found, the result describes the version and includes details of the document type, editor, version date, and published status.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IContent?, ContentVersionOperationStatus> attempt =
            await _contentVersionService.GetAsync(id);

        return attempt.Success
            ? Ok(_umbracoMapper.Map<DocumentVersionResponseModel>(attempt.Result))
            : MapFailure(attempt.Status);
    }
}
