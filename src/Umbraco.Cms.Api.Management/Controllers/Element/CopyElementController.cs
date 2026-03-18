using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// API controller responsible for handling copy operations on elements in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CopyElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize element copy operations.</param>
    /// <param name="elementEditingService">Service responsible for element editing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CopyElementController(
        IAuthorizationService authorizationService,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a copy of an element identified by the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element to copy.</param>
    /// <param name="copyElementRequestModel">The model containing the target location for the copy.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the copy operation.</returns>
    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Copies an element.")]
    [EndpointDescription("Creates a duplicate of an existing element identified by the provided Id.")]
    public async Task<IActionResult> Copy(CancellationToken cancellationToken, Guid id, CopyElementRequestModel copyElementRequestModel)
    {
        // Check Copy permission on source element
        AuthorizationResult sourceAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementCopy.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!sourceAuthorizationResult.Succeeded)
        {
            return Forbidden();
        }

        // Check Create permission on target (where we're copying to)
        AuthorizationResult targetAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementNew.ActionLetter, copyElementRequestModel.Target?.Id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!targetAuthorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IElement?, ContentEditingOperationStatus> result = await _elementEditingService.CopyAsync(
            id,
            copyElementRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyElementController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
