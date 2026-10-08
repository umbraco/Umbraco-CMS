using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// API controller responsible for handling move-to-recycle-bin operations on elements in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class MoveToRecycleBinElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveToRecycleBinElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize recycle bin operations.</param>
    /// <param name="elementEditingService">Service responsible for element editing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public MoveToRecycleBinElementController(
        IAuthorizationService authorizationService,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Moves an element identified by the specified unique identifier to the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element to move to the recycle bin.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the operation.</returns>
    [HttpPut("{id:guid}/move-to-recycle-bin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves an element to the recycle bin.")]
    [EndpointDescription("Moves an element identified by the provided Id to the recycle bin.")]
    public async Task<IActionResult> MoveToRecycleBin(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementDelete.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentEditingOperationStatus> result = await _elementEditingService.MoveToRecycleBinAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Result);
    }
}
