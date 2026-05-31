using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

/// <summary>
/// Provides API endpoints for managing document type folders identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentTypeFolderController : DocumentTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDocumentTypeFolderController"/> class, which manages document type folders by their unique key.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentTypeContainerService">Service used to manage content type containers (folders) within the system.</param>
    public ByKeyDocumentTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }

    /// <summary>
    /// Retrieves a document type folder by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document type folder to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the folder details if found; otherwise, a <see cref="ProblemDetails"/> with status 404 if not found.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document type folder.")]
    [EndpointDescription("Gets a document type folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
