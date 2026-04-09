using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

/// <summary>
/// Controller for deleting document type folders.
/// </summary>
[ApiVersion("1.0")]
public class DeleteDocumentTypeFolderController : DocumentTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentTypeFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security, used to authorize access to this controller's actions.</param>
    /// <param name="contentTypeContainerService">Service for managing content type folders (containers) within the document type section.</param>
    public DeleteDocumentTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }

    /// <summary>
    /// Deletes a document type folder identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the document type folder to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a document type folder.")]
    [EndpointDescription("Deletes a document type folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id) => await DeleteFolderAsync(id);
}
