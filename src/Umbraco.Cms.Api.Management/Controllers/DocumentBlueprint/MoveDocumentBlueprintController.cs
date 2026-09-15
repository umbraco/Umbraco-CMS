using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// Controller for handling move operations on document blueprints.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentBlueprints)]
public class MoveDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="contentBlueprintEditingService">The service used to edit content blueprints. This dependency is injected.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information. This dependency is injected.</param>
    /// <param name="authorizationService">The authorization service.</param>
    [ActivatorUtilitiesConstructor]
    public MoveDocumentBlueprintController(
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="contentBlueprintEditingService">The service used to edit content blueprints. This dependency is injected.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information. This dependency is injected.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public MoveDocumentBlueprintController(IContentBlueprintEditingService contentBlueprintEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            contentBlueprintEditingService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Moves the specified document blueprint to a new location.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document blueprint to move.</param>
    /// <param name="requestModel">The model containing the target location information.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a document blueprint.")]
    [EndpointDescription("Moves a document blueprint identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(CancellationToken cancellationToken, Guid id, MoveDocumentBlueprintRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys([id, requestModel.Target?.Id]),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<ContentEditingOperationStatus> result = await _contentBlueprintEditingService.MoveAsync(id, requestModel.Target?.Id, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Result);
    }
}
