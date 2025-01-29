using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class DeleteStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteStylesheetController(
        IStylesheetService stylesheetService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _stylesheetService = stylesheetService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetOperationStatus operationStatus = await _stylesheetService.DeleteAsync(path, CurrentUserKey(_backOfficeSecurityAccessor));

        return operationStatus is StylesheetOperationStatus.Success
            ? Ok()
            : StylesheetOperationStatusResult(operationStatus);
    }
}
