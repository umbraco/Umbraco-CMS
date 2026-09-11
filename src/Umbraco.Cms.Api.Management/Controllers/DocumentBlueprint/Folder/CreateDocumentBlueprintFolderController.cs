using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Folder;

/// <summary>
/// Controller responsible for handling requests to create folders for document blueprints.
/// </summary>
[ApiVersion("1.0")]
public class CreateDocumentBlueprintFolderController : DocumentBlueprintFolderControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentBlueprintFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    /// <param name="contentBlueprintContainerService">Service for managing content blueprint containers.</param>
    /// <param name="authorizationService">The authorization service.</param>
    [ActivatorUtilitiesConstructor]
    public CreateDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService,
        IAuthorizationService authorizationService)
        : base(backOfficeSecurityAccessor, contentBlueprintContainerService)
        => _authorizationService = authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentBlueprintFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    /// <param name="contentBlueprintContainerService">Service for managing content blueprint containers.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public CreateDocumentBlueprintFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentBlueprintContainerService contentBlueprintContainerService)
        : this(
            backOfficeSecurityAccessor,
            contentBlueprintContainerService,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Creates a new document blueprint folder with the specified name and parent location.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="createFolderRequestModel">The model containing details for the folder to create.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation result.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a document blueprint folder.")]
    [EndpointDescription("Creates a new document blueprint folder with the provided name and parent location.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateFolderRequestModel createFolderRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys(createFolderRequestModel.Parent?.Id),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await CreateFolderAsync<ByKeyDocumentBlueprintFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey));
    }
}
