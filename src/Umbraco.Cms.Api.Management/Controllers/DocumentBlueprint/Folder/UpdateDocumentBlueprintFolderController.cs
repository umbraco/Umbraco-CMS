using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;

/// <summary>
/// Controller for updating folders that contain document blueprints.
/// </summary>
[ApiVersion("1.0")]
public class UpdateDocumentBlueprintFolderController : DocumentBlueprintFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentBlueprintFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentBlueprintContainerService">Service used to manage content blueprint folders (containers).</param>
    public UpdateDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService)
        : base(backOfficeSecurityAccessor, contentBlueprintContainerService)
    {
    }

    /// <summary>
    /// Updates the specified document blueprint folder with new details.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document blueprint folder to update.</param>
    /// <param name="updateFolderResponseModel">The updated folder details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a document blueprint folder.")]
    [EndpointDescription("Updates a document blueprint folder identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, UpdateFolderResponseModel updateFolderResponseModel)
        => await UpdateFolderAsync(id, updateFolderResponseModel);
}
