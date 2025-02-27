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

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class CreateDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IDocumentBlueprintEditingPresentationFactory _blueprintEditingPresentationFactory;
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDocumentBlueprintController(
        IDocumentBlueprintEditingPresentationFactory blueprintEditingPresentationFactory,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _blueprintEditingPresentationFactory = blueprintEditingPresentationFactory;
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
