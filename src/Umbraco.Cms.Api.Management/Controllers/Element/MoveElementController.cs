using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class MoveElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public MoveElementController(
        IAuthorizationService authorizationService,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(CancellationToken cancellationToken, Guid id, MoveElementRequestModel moveElementRequestModel)
    {
        // Check Move permission on source element
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
            ElementPermissionResource.WithKeys(ActionElementNew.ActionLetter, moveElementRequestModel.Target?.Id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!targetAuthorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentEditingOperationStatus> result = await _elementEditingService.MoveAsync(
            id,
            moveElementRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Result);
    }
}
