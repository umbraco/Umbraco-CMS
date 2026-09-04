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
/// Provides API endpoints for managing document blueprint folders identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentBlueprintFolderController : DocumentBlueprintFolderControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDocumentBlueprintFolderController"/> class, which manages document blueprint folders by their unique key.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="contentBlueprintContainerService">Service used to manage content blueprint containers (folders).</param>
    public ByKeyDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService,
        IAuthorizationService authorizationService)
        : base(backOfficeSecurityAccessor, contentBlueprintContainerService)
        => _authorizationService = authorizationService;

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public ByKeyDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService)
        : this(
            backOfficeSecurityAccessor,
            contentBlueprintContainerService,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document blueprint folder.")]
    [EndpointDescription("Gets a document blueprint folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys(id),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await GetFolderAsync(id);
    }
}
