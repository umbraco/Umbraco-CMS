using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;

    /// <summary>
    /// Controller responsible for handling HTTP requests related to updating document type folders in the Umbraco CMS.
    /// </summary>
[ApiVersion("1.0")]
public class UpdateDocumentTypeFolderController : DocumentTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentTypeFolderController"/> class, which handles requests for updating document type folders in the Umbraco back office.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentTypeContainerService">Service used to manage content type containers (folders) within the system.</param>
    public UpdateDocumentTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentTypeContainerService contentTypeContainerService)
        : base(backOfficeSecurityAccessor, contentTypeContainerService)
    {
    }

    /// <summary>
    /// Updates a document type folder identified by the provided <paramref name="id"/> with the details from the request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document type folder to update.</param>
    /// <param name="updateFolderResponseModel">The request model containing the updated folder details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a document type folder.")]
    [EndpointDescription("Updates a document type folder identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateFolderResponseModel updateFolderResponseModel)
        => await UpdateFolderAsync(id, updateFolderResponseModel);
}
