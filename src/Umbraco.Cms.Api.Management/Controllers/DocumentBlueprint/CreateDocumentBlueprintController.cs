using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// API controller responsible for handling requests to create document blueprints in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class CreateDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IDocumentBlueprintEditingPresentationFactory _blueprintEditingPresentationFactory;
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="blueprintEditingPresentationFactory">Factory used to create document blueprint editing presentations.</param>
    /// <param name="contentBlueprintEditingService">Service used for editing document blueprints.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    public CreateDocumentBlueprintController(
        IDocumentBlueprintEditingPresentationFactory blueprintEditingPresentationFactory,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _blueprintEditingPresentationFactory = blueprintEditingPresentationFactory;
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
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
        ContentBlueprintCreateModel model = _blueprintEditingPresentationFactory.MapCreateModel(requestModel);

        // We don't need to validate user access because we "only" require access to the Settings section to create new blueprints from scratch
        Attempt<ContentCreateResult, ContentEditingOperationStatus> result = await _contentBlueprintEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDocumentBlueprintController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
