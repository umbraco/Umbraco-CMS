using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// API controller responsible for restoring element folders from the recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class RestoreElementFolderRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementContainerService _elementContainerService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreElementFolderRecycleBinController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize restore operations.</param>
    /// <param name="elementContainerService">Service for managing element containers.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public RestoreElementFolderRecycleBinController(
        IAuthorizationService authorizationService,
        IElementContainerService elementContainerService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementContainerService = elementContainerService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Restores an element folder from the recycle bin to its original location or a specified parent.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element folder to restore.</param>
    /// <param name="moveFolderRequestModel">The model containing the target location for the restored folder.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the restore operation.</returns>
    [HttpPut("folder/{id:guid}/restore")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Restores an element folder from the recycle bin.")]
    [EndpointDescription("Restores an element folder from the recycle bin to its original location or a specified parent.")]
    public async Task<IActionResult> RestoreFolder(CancellationToken cancellationToken, Guid id, MoveFolderRequestModel moveFolderRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.RecycleBin(ActionElementMove.ActionLetter),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<EntityContainerOperationStatus> result = await _elementContainerService.RestoreAsync(
            id,
            moveFolderRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Result);
    }
}
