using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder;

/// <summary>
/// Controller for retrieving element folders by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyElementFolderController : ElementFolderControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyElementFolderController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element folder resources.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="elementContainerService">Service for managing element containers.</param>
    public ByKeyElementFolderController(
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementContainerService elementContainerService)
        : base(backOfficeSecurityAccessor, elementContainerService) =>
        _authorizationService = authorizationService;

    /// <summary>
    /// Retrieves an element folder by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element folder to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the element folder if found.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets an element folder.")]
    [EndpointDescription("Gets an element folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        return await GetFolderAsync(id);
    }
}
