using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class MoveToRecycleBinDocumentController : DocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public MoveToRecycleBinDocumentController(IContentEditingService contentEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToRecycleBin(Guid key)
    {
        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.MoveToRecycleBinAsync(key, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
