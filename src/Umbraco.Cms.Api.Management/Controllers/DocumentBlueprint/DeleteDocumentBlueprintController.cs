using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

[ApiVersion("1.0")]
public class DeleteDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteDocumentBlueprintController(IContentBlueprintEditingService contentBlueprintEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentBlueprintEditingService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
