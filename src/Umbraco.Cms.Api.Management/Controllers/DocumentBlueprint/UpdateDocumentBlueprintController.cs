using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class UpdateDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IDocumentBlueprintEditingPresentationFactory _blueprintEditingPresentationFactory;
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateDocumentBlueprintController(
        IDocumentBlueprintEditingPresentationFactory blueprintEditingPresentationFactory,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _blueprintEditingPresentationFactory = blueprintEditingPresentationFactory;
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, UpdateDocumentBlueprintRequestModel requestModel)
    {
        ContentBlueprintUpdateModel model = _blueprintEditingPresentationFactory.MapUpdateModel(requestModel);

        // We don't need to validate user access because we "only" require access to the Settings section to update blueprints
        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await _contentBlueprintEditingService.UpdateAsync(id, model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
