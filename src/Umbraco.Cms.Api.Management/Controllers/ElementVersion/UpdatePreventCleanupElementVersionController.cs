using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

[ApiVersion("1.0")]
public class UpdatePreventCleanupElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdatePreventCleanupElementVersionController(
        IElementVersionService elementVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _elementVersionService = elementVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/prevent-cleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set(CancellationToken cancellationToken, Guid id, bool preventCleanup)
    {
        Attempt<ContentVersionOperationStatus> attempt =
            await _elementVersionService.SetPreventCleanupAsync(id, preventCleanup, CurrentUserKey(_backOfficeSecurityAccessor));

        return attempt.Success
            ? Ok()
            : MapFailure(attempt.Result);
    }
}
