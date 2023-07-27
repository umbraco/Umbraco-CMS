using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class PublishDocumentController : DocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public PublishDocumentController(IContentEditingService contentEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("publish/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id, [FromQuery] string[]? cultures)
    {
        Attempt<ContentPublishingOperationStatus> attempt;
        if (cultures is null)
        {
            attempt = await _contentEditingService.PublishAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));
        }
        else
        {
            attempt = await _contentEditingService.PublishAsync(id, CurrentUserKey(_backOfficeSecurityAccessor), cultures);
        }

        return attempt.Success ? Ok() : ContentPublishingOperationStatusResult(attempt.Result);
    }
}
