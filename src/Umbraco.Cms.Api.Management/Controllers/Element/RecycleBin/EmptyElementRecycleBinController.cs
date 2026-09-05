using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
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
/// API controller responsible for emptying the element recycle bin in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class EmptyElementRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IElementContainerService _elementContainerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyElementRecycleBinController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize recycle bin operations.</param>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="elementContainerService">Service for managing element containers.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public EmptyElementRecycleBinController(
        IAuthorizationService authorizationService,
        IEntityService entityService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementContainerService elementContainerService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _elementContainerService = elementContainerService;
    }

    /// <summary>
    /// Permanently deletes all elements in the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.</returns>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Empties the element recycle bin.")]
    [EndpointDescription("Permanently deletes all elements in the recycle bin. This operation cannot be undone.")]
    public async Task<IActionResult> EmptyRecycleBin(CancellationToken cancellationToken)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.RecycleBin(ActionElementDelete.ActionLetter),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<EntityContainerOperationStatus> result = await _elementContainerService.EmptyRecycleBinAsync(CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result.Result);
    }
}
