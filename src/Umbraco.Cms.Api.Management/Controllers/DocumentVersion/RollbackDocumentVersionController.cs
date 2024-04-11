using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

[ApiVersion("1.0")]
public class RollbackDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RollbackDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentVersionService = contentVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Rollback(CancellationToken cancellationToken, Guid id, string? culture)
    {
        Attempt<ContentVersionOperationStatus> attempt =
            await _contentVersionService.RollBackAsync(id, culture, CurrentUserKey(_backOfficeSecurityAccessor));

        return attempt.Success
            ? Ok()
            : MapFailure(attempt.Result);
    }
}
