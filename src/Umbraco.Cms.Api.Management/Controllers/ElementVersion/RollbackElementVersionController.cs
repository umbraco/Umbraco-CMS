using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

[ApiVersion("1.0")]
public class RollbackElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RollbackElementVersionController(
        IElementVersionService elementVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _elementVersionService = elementVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Rollback(CancellationToken cancellationToken, Guid id, string? culture)
    {
        Attempt<IElement?, ContentVersionOperationStatus> getContentAttempt = await _elementVersionService.GetAsync(id);
        if (getContentAttempt.Success is false || getContentAttempt.Result is null)
        {
            return MapFailure(getContentAttempt.Status);
        }


        // TODO ELEMENTS: handle auth
        // IElement element = getContentAttempt.Result;
        // AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
        //     User,
        //     ContentPermissionResource.WithKeys(ActionRollback.ActionLetter, element.Key),
        //     AuthorizationPolicies.ContentPermissionByResource);
        //
        // if (!authorizationResult.Succeeded)
        // {
        //     return Forbidden();
        // }

        Attempt<ContentVersionOperationStatus> rollBackAttempt =
            await _elementVersionService.RollBackAsync(id, culture, CurrentUserKey(_backOfficeSecurityAccessor));

        return rollBackAttempt.Success
            ? Ok()
            : MapFailure(rollBackAttempt.Result);
    }
}
