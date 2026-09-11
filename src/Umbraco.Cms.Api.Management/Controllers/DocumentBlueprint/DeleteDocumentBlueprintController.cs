using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// API controller responsible for handling requests to delete document blueprints in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentBlueprints)]
public class DeleteDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentBlueprintController"/> class, which handles requests to delete document blueprints.
    /// </summary>
    /// <param name="contentBlueprintEditingService">Service used for editing content blueprints.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="authorizationService">The authorization service.</param>
    [ActivatorUtilitiesConstructor]
    public DeleteDocumentBlueprintController(
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="contentBlueprintEditingService">Service used for editing content blueprints.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public DeleteDocumentBlueprintController(IContentBlueprintEditingService contentBlueprintEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            contentBlueprintEditingService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a document blueprint.")]
    [EndpointDescription("Deletes a document blueprint identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys(id),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentBlueprintEditingService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
