using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Version;

[ApiVersion("1.0")]
public class UpdatePreventCleanupDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdatePreventCleanupDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(contentVersionService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/preventCleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set(Guid id, bool preventCleanup)
    {
        Attempt<ContentVersionOperationStatus> attempt =
            await ContentVersionService.SetPreventCleanupAsync(id, preventCleanup, CurrentUserKey(_backOfficeSecurityAccessor));

        return attempt.Success is true
            ? Ok()
            : MapFailure(attempt.Result);
    }
}
