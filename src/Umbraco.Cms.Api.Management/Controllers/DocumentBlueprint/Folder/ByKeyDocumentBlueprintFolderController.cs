using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;

/// <summary>
/// Provides API endpoints for managing document blueprint folders identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentBlueprintFolderController : DocumentBlueprintFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDocumentBlueprintFolderController"/> class, which manages document blueprint folders by their unique key.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentBlueprintContainerService">Service used to manage content blueprint containers (folders).</param>
    public ByKeyDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService)
        : base(backOfficeSecurityAccessor, contentBlueprintContainerService)
    {
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document blueprint folder.")]
    [EndpointDescription("Gets a document blueprint folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
