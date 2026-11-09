using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// API controller responsible for handling requests to create document blueprints in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentBlueprints)]
public class CreateDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IDocumentBlueprintEditingPresentationFactory _blueprintEditingPresentationFactory;
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="blueprintEditingPresentationFactory">Factory used to create document blueprint editing presentations.</param>
    /// <param name="contentBlueprintEditingService">Service used for editing document blueprints.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    /// <param name="authorizationService">The authorization service.</param>
    [ActivatorUtilitiesConstructor]
    public CreateDocumentBlueprintController(
        IDocumentBlueprintEditingPresentationFactory blueprintEditingPresentationFactory,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _blueprintEditingPresentationFactory = blueprintEditingPresentationFactory;
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="blueprintEditingPresentationFactory">Factory used to create document blueprint editing presentations.</param>
    /// <param name="contentBlueprintEditingService">Service used for editing document blueprints.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public CreateDocumentBlueprintController(
        IDocumentBlueprintEditingPresentationFactory blueprintEditingPresentationFactory,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            blueprintEditingPresentationFactory,
            contentBlueprintEditingService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Creates a new document blueprint using the specified request model.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="requestModel">The model containing the configuration for the document blueprint to create.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation result.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new document blueprint.")]
    [EndpointDescription("Creates a new document blueprint with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateDocumentBlueprintRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys(requestModel.Parent?.Id),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        ContentBlueprintCreateModel model = _blueprintEditingPresentationFactory.MapCreateModel(requestModel);

        Attempt<ContentCreateResult, ContentEditingOperationStatus> result = await _contentBlueprintEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDocumentBlueprintController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
