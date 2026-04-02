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

/// <summary>
/// API controller responsible for handling requests to delete stylesheet resources.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class DeleteStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteStylesheetController"/> class, responsible for handling stylesheet deletion requests in the management API.
    /// </summary>
    /// <param name="stylesheetService">Service used to manage and delete stylesheets.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context, used to authorize deletion operations.</param>
    public DeleteStylesheetController(
        IStylesheetService stylesheetService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _stylesheetService = stylesheetService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>Deletes a stylesheet identified by the provided path.</summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <param name="path">The path of the stylesheet to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a stylesheet.")]
    [EndpointDescription("Deletes a stylesheet identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetOperationStatus operationStatus = await _stylesheetService.DeleteAsync(path, CurrentUserKey(_backOfficeSecurityAccessor));

        return operationStatus is StylesheetOperationStatus.Success
            ? Ok()
            : StylesheetOperationStatusResult(operationStatus);
    }
}
