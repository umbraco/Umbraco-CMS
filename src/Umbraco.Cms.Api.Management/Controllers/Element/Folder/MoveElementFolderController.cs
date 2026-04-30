using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Folder;

/// <summary>
/// API controller responsible for handling move operations on element folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class MoveElementFolderController : ElementFolderControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementContainerService _elementContainerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveElementFolderController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize element folder move operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="elementContainerService">Service for managing element containers.</param>
    public MoveElementFolderController(
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementContainerService elementContainerService)
        : base(backOfficeSecurityAccessor, elementContainerService)
    {
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementContainerService = elementContainerService;
    }

    /// <summary>
    /// Moves an element folder identified by the specified unique identifier to a different location.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element folder to move.</param>
    /// <param name="moveFolderRequestModel">The model containing the target location for the move.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the move operation.</returns>
    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves an element folder.")]
    [EndpointDescription("Moves an element folder identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(CancellationToken cancellationToken, Guid id, MoveFolderRequestModel moveFolderRequestModel)
    {
        // Check Move permission on source folder
        AuthorizationResult sourceAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementMove.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!sourceAuthorizationResult.Succeeded)
        {
            return Forbidden();
        }

        // Check Create permission on target (where we're moving to)
        AuthorizationResult targetAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementNew.ActionLetter, moveFolderRequestModel.Target?.Id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!targetAuthorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<EntityContainerOperationStatus> result = await _elementContainerService
            .MoveAsync(id, moveFolderRequestModel.Target?.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Result);
    }
}
