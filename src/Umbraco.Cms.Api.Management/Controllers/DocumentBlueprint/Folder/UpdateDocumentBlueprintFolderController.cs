using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;

/// <summary>
/// Controller for updating folders that contain document blueprints.
/// </summary>
[ApiVersion("1.0")]
public class UpdateDocumentBlueprintFolderController : DocumentBlueprintFolderControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentBlueprintFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentBlueprintContainerService">Service used to manage content blueprint folders (containers).</param>
    /// <param name="authorizationService">The authorization service.</param>
    [ActivatorUtilitiesConstructor]
    public UpdateDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService,
        IAuthorizationService authorizationService)
        : base(backOfficeSecurityAccessor, contentBlueprintContainerService)
        => _authorizationService = authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentBlueprintFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentBlueprintContainerService">Service used to manage content blueprint folders (containers).</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public UpdateDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService)
        : this(
            backOfficeSecurityAccessor,
            contentBlueprintContainerService,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
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
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys(id),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await UpdateFolderAsync(id, updateFolderResponseModel);
    }
}
