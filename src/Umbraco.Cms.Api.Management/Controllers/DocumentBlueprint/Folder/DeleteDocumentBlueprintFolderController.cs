using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;

/// <summary>
/// API controller responsible for handling the deletion of document blueprint folders in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteDocumentBlueprintFolderController : DocumentBlueprintFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentBlueprintFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentBlueprintContainerService">Service used to manage content blueprint folders (containers).</param>
    public DeleteDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService)
        : base(backOfficeSecurityAccessor, contentBlueprintContainerService)
    {
    }

    /// <summary>
    /// Deletes a document blueprint folder identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the document blueprint folder to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a document blueprint folder.")]
    [EndpointDescription("Deletes a document blueprint folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id) => await DeleteFolderAsync(id);
}
