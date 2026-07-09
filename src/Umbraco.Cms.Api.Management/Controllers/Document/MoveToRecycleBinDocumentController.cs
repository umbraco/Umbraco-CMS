using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller for moving documents to the recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class MoveToRecycleBinDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveToRecycleBinDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions for moving documents to the recycle bin.</param>
    /// <param name="contentEditingService">Service responsible for editing and managing content documents.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and user information.</param>
    public MoveToRecycleBinDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/move-to-recycle-bin")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a document to the recycle bin.")]
    [EndpointDescription("Moves a document identified by the provided Id to the recycle bin.")]
    public async Task<IActionResult> MoveToRecycleBin(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionDelete.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.MoveToRecycleBinAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
